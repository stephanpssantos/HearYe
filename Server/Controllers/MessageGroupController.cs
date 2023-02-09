// <copyright file="MessageGroupController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.Identity.Web.Resource;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to post message groups.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class MessageGroupController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly ILogger<MessageGroupController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageGroupController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public MessageGroupController(HearYeContext db, ILogger<MessageGroupController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// GET: api/messagegroup/[id]; <br />
        /// Return record for specified message group.
        /// </summary>
        /// <param name="id">Message group id.</param>
        /// <returns>200 (with a message group object), 401, or 404.</returns>
        [HttpGet("{id:int}", Name = nameof(GetMessageGroup))]
        [ProducesResponseType(200, Type = typeof(MessageGroup))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroup(int id)
        {
            MessageGroup? messageGroup = await this.db.MessageGroups!.Where(mg => mg.Id == id).FirstOrDefaultAsync();

            if (messageGroup == null)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, id);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            return this.Ok(messageGroup);
        }

        /// <summary>
        /// GET: api/messagegroup/members/[id]; <br />
        /// Returns all members of a specified message group.
        /// </summary>
        /// <param name="id">Id of the specified message group.</param>
        /// <returns>200 (with a list of member group member objects), 400, 401, or 404.</returns>
        [HttpGet("members/{id:int}", Name = nameof(GetMessageGroupMembers))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroupMemberWithName>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroupMembers(int id)
        {
            if (id == 0)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, id);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            IEnumerable<MessageGroupMemberWithName?> mg = await this.db.MessageGroupMembers!
                .Where(mgm => mgm.MessageGroupId == id)
                .Include(mgm => mgm.User)
                .Select(mgm => new MessageGroupMemberWithName()
                {
                    Id = mgm.Id,
                    MessageGroupId = mgm.MessageGroupId,
                    MessageGroupRoleId = mgm.MessageGroupRoleId,
                    UserId = mgm.UserId,
                    UserName = mgm.User != null ? mgm.User.DisplayName : "Unknown",
                })
                .ToListAsync();

            return this.Ok(mg);
        }

        /// <summary>
        /// POST: api/messagegroup/new; <br />
        /// Request new message group. Adds the requesting user as an admin of the new group.
        /// </summary>
        /// <param name="groupName">Group name included in request body in JSON format.</param>
        /// <returns>201, 400, 401.</returns>
        [HttpPost("new")]
        [ProducesResponseType(201, Type = typeof(MessageGroup))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewMessageGroup([FromBody] string groupName)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrEmpty(groupName))
            {
                return this.BadRequest("New group name required.");
            }

            if (groupName.Length > 50 || groupName.Length < 4)
            {
                return this.BadRequest("New group name must be between 4 and 50 characters.");
            }

            using var transaction = this.db.Database.BeginTransaction();

            try
            {
                // Make group
                MessageGroup newGroup = new ()
                {
                    MessageGroupName = groupName,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                };
                EntityEntry<MessageGroup> newGroupEntry = await this.db.MessageGroups!.AddAsync(newGroup);

                int completedGroup = await this.db.SaveChangesAsync();
                if (completedGroup != 1)
                {
                    transaction.Rollback();
                    this.logger.LogError("New message group transaction rolled back.");
                    this.logger.LogError(JsonSerializer.Serialize(newGroup, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to create new message group.");
                }

                // Assign user requesting new group as group admin
                MessageGroupMember newGroupAdmin = new ()
                {
                    MessageGroupId = newGroupEntry.Entity.Id,
                    MessageGroupRoleId = 1, // Admin
                    UserId = claimId,
                };
                EntityEntry<MessageGroupMember> newGroupAdminEntry = await this.db.MessageGroupMembers!.AddAsync(newGroupAdmin);

                int completedAdmin = await this.db.SaveChangesAsync();
                if (completedAdmin != 1)
                {
                    transaction.Rollback();
                    this.logger.LogError("New message group transaction rolled back. Could not add admin.");
                    this.logger.LogError(JsonSerializer.Serialize(newGroupAdmin, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to add users to group.");
                }

                transaction.Commit();

                return this.CreatedAtRoute(
                    routeName: nameof(this.GetMessageGroup),
                    routeValues: new { id = newGroupEntry.Entity.Id },
                    value: newGroupEntry.Entity);
            }
            catch (Exception ex)
            {
                // Log this exception.
                transaction.Rollback();
                this.logger.LogError("Error when creating new message group.");
                this.logger.LogError(ex.Message);
                return this.BadRequest("Failed to create new message group.");
            }
        }

        /// <summary>
        /// PUT: api/messagegroup/setrole; <br />
        /// Set a role for a user within a message group.
        /// User must already be a member of the group.
        /// Requester must be a group admin.
        /// </summary>
        /// <param name="mgm">MessageGroupMember object included in request body in JSON format.</param>
        /// <returns>204, 401, or 400.</returns>
        [HttpPut("setrole")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetMessageGroupRole([FromBody] MessageGroupMember mgm)
        {
            if (mgm == null || !this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, mgm.MessageGroupId);

            if (roleId != 1)
            {
                return this.Unauthorized();
            }

            using var transaction = this.db.Database.BeginTransaction();
            try
            {
                EntityEntry<MessageGroupMember> mgmUpdate = this.db.MessageGroupMembers!.Update(mgm);

                if (mgmUpdate.State == EntityState.Added)
                {
                    transaction.Rollback();
                    return this.BadRequest("Specified user is not group member.");
                }

                int completed = await this.db.SaveChangesAsync();

                if (completed != 1)
                {
                    transaction.Rollback();
                    this.logger.LogError("Set mg role transaction rolled back.");
                    this.logger.LogError(JsonSerializer.Serialize(mgm), CustomJsonOptions.IgnoreCycles());
                    return this.BadRequest("Failed to set message group member role.");
                }

                transaction.Commit();
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                this.logger.LogError("Error when setting message group role.");
                this.logger.LogError(ex.Message);
                this.logger.LogError(JsonSerializer.Serialize(mgm), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Error when setting message group member role.");
            }
        }

        /// <summary>
        /// DELETE: api/messagegroup/[id]; <br />
        /// Delete specified message group. Requester must be a group admin.
        /// Soft delete only, does not remove database record.
        /// </summary>
        /// <param name="id">Id of the message group to delete.</param>
        /// <returns>204, 400, 401, 404.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroup(int id)
        {
            MessageGroup? mg = await this.db.MessageGroups!.Where(mg => mg.Id == id).FirstOrDefaultAsync();

            if (mg == null || id == 0)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, id);

            if (roleId != 1)
            {
                return this.Unauthorized();
            }

            mg.IsDeleted = true;
            mg.DeletedDate = DateTimeOffset.Now;
            this.db.MessageGroups!.Update(mg);

            int completed = await this.db.SaveChangesAsync();

            if (completed == 1)
            {
                return this.NoContent();
            }
            else
            {
                this.logger.LogError("Error when deleting message group.");
                this.logger.LogError(JsonSerializer.Serialize(mg), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Message group found but failed to delete.");
            }
        }

        /// <summary>
        /// DELETE: api/messagegroup/member/[id]; <br />
        /// Delete specified message group member. Requester must be the user or a group admin.
        /// </summary>
        /// <param name="id">Id of the message group member to delete.</param>
        /// <returns>204, 400, 401, 404.</returns>
        [HttpDelete("member/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroupMember(int id)
        {
            MessageGroupMember? mgm = await this.db.MessageGroupMembers!.Where(mgm => mgm.MessageGroupId == id).FirstOrDefaultAsync();

            if (mgm == null || id == 0)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, mgm.MessageGroupId);

            if (roleId != 1 && mgm.UserId != claimId)
            {
                return this.Unauthorized();
            }

            this.db.MessageGroupMembers!.Remove(mgm);

            int completed = await this.db.SaveChangesAsync();

            if (completed == 1)
            {
                return this.NoContent();
            }
            else
            {
                this.logger.LogError("Error when deleting message group member.");
                this.logger.LogError(JsonSerializer.Serialize(mgm), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Message group member found but failed to delete.");
            }
        }
    }
}
