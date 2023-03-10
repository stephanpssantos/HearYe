// <copyright file="MessageGroupShortcutController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Text.Json;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to a user's group shortcuts.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class MessageGroupShortcutController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly ILogger<MessageGroupShortcutController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageGroupShortcutController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public MessageGroupShortcutController(HearYeContext db, ILogger<MessageGroupShortcutController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// GET: api/messagegroupshortcut/[id]; <br />
        /// Get all message groups shortcutted by a user.
        /// </summary>
        /// <param name="id">Id of the specified user.</param>
        /// <returns>200 (with a list of message groups), or 401.</returns>
        [HttpGet("{id:int}", Name = nameof(GetMessageGroupShortcuts))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroup>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMessageGroupShortcuts(int id)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0 || id != claimId)
            {
                return this.Unauthorized();
            }

            IEnumerable<MessageGroup?> messageGroupShortcuts = await this.db.MessageGroupShortcuts!
                .Include(mgs => mgs.MessageGroup)
                .Where(mgs => mgs.UserId == claimId)
                .Select(mgs => mgs.MessageGroup)
                .ToListAsync();

            return this.Ok(messageGroupShortcuts);
        }

        /// <summary>
        /// POST: api/messagegroupshortcut/new; <br />
        /// Create a new message group shortcut.
        /// </summary>
        /// <param name="shortcut">MessageGroupShortcut object included in request body in JSON format.</param>
        /// <returns>204, 400, or 401.</returns>
        [HttpPost("new", Name = nameof(NewMessageGroupShortcut))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewMessageGroupShortcut([FromBody] MessageGroupShortcut shortcut)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, shortcut.MessageGroupId);

            if (roleId == 0 || shortcut.UserId != claimId)
            {
                return this.Unauthorized();
            }

            try
            {
                EntityEntry<MessageGroupShortcut> newShortcut = await this.db.MessageGroupShortcuts!.AddAsync(shortcut);
                int completed = await this.db.SaveChangesAsync();
                if (completed != 1)
                {
                    this.logger.LogError("New message group shortcut failed.");
                    this.logger.LogError(JsonSerializer.Serialize(shortcut, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to create shortcut.");
                }

                return this.NoContent();
            }
            catch (DbUpdateException exception)
            {
                // Cannot insert duplicate key row in object error
                var ex = exception.InnerException as SqlException;
                if (ex is not null && ex.Number == 2601)
                {
                    return this.BadRequest("Duplicate shortcut exists.");
                }
                else
                {
                    throw new Exception(exception.Message);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error when creating new message group shortcut.");
                this.logger.LogError(ex.Message);
                this.logger.LogError(JsonSerializer.Serialize(shortcut, CustomJsonOptions.IgnoreCycles()));
                return this.BadRequest("Error when creating message group shortcut.");
            }
        }

        /// <summary>
        /// DELETE: api/messagegroupshortcut/delete?userId=[userId]&amp;groupId=[groupId]; <br />
        /// Delete the specified message group shortcut. Must be the user who owns the shortcut.
        /// </summary>
        /// <param name="userId">User id of user who owns the shortcut.</param>
        /// <param name="groupId">Message group id of a message group.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpDelete("delete", Name = nameof(DeleteMessageGroupShortcut))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroupShortcut(int userId, int groupId)
        {
            if (userId < 1 || groupId < 1)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            MessageGroupShortcut? mgs = await this.db.MessageGroupShortcuts!
                .Where(mgs => mgs.UserId == userId && mgs.MessageGroupId == groupId)
                .FirstOrDefaultAsync();

            if (mgs == null)
            {
                return this.NotFound();
            }
            else if (claimId != mgs.UserId)
            {
                return this.Unauthorized("Not shortcut owner.");
            }

            try
            {
                this.db.MessageGroupShortcuts!.Remove(mgs);
                int completed = await this.db.SaveChangesAsync();

                if (completed == 1)
                {
                    return this.NoContent();
                }
                else
                {
                    this.logger.LogError("Failed to delete message group shortcut.");
                    this.logger.LogError(JsonSerializer.Serialize(mgs, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to delete shortcut.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error when deleting message group shortcut.");
                this.logger.LogError(ex.Message);
                this.logger.LogError(JsonSerializer.Serialize(mgs, CustomJsonOptions.IgnoreCycles()));
                return this.BadRequest("Error when deleting shortcut.");
            }
        }
    }
}
