﻿// <copyright file="MessageGroupInvitationController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to message group invitations.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class MessageGroupInvitationController : ControllerBase
    {
        private readonly HearYeContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageGroupInvitationController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        public MessageGroupInvitationController(HearYeContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// GET: api/messagegroupinvitation/[id]; <br />
        /// Returns the specified message group invitation.
        /// Requester must either be the invitee or inviter.
        /// </summary>
        /// <param name="id">Id of the requested message group invitation.</param>
        /// <returns>200 (with a message group invitation object), 401, or 404.</returns>
        [HttpGet("{id:int}", Name = nameof(GetMessageGroupInvitation))]
        [ProducesResponseType(200, Type = typeof(MessageGroupInvitation))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroupInvitation(int id)
        {
            MessageGroupInvitation? messageGroupInvitation = await this.db.MessageGroupInvitations!
                .Where(mgi => mgi.Id == id).FirstOrDefaultAsync();

            if (messageGroupInvitation == null)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserInviteAuthCheck(this.db, claimId, id);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            return this.Ok(messageGroupInvitation);
        }

        /// <summary>
        /// GET: api/messagegroupinvitation/user/[id]; <br />
        /// Get all message group invitations that a user has sent or received.
        /// </summary>
        /// <param name="userId">Id of the specified user.</param>
        /// <returns>200 (with a list of member group invitations), or 401.</returns>
        [HttpGet("user/{id:int}", Name = nameof(GetMessageGroupInvitations))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroupInvitation>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMessageGroupInvitations(int userId)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0 || userId != claimId)
            {
                return this.Unauthorized();
            }

            IEnumerable<MessageGroupInvitation> messageGroupInvitations = await this.db.MessageGroupInvitations!
                .Where(mgi => (mgi.InvitationActive == true)
                    && ((mgi.InvitingUserId == userId)
                    || (mgi.InvitedUserId == userId)))
                .ToListAsync();

            return this.Ok(messageGroupInvitations);
        }

        /// <summary>
        /// POST: api/messagegroupinvitations/new; <br />
        /// Create new message group invitation.
        /// </summary>
        /// <param name="invite">MessageGroupInvitation object included in request body in JSON format.</param>
        /// <returns>201, 400, or 401.</returns>
        [HttpPost("new")]
        [ProducesResponseType(201, Type = typeof(MessageGroupInvitation))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewMessageGroupInvitation([FromBody] MessageGroupInvitation invite)
        {
            if (invite == null || !this.ModelState.IsValid || invite.InvitationAccepted == true || invite.InvitationActive == false)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, invite.MessageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            MessageGroupMember? invitee = await this.db.MessageGroupMembers!
                .Where(members => (members.MessageGroupId == invite.MessageGroupId) && (members.UserId == invite.InvitedUserId))
                .FirstOrDefaultAsync();

            if (invitee != null)
            {
                return this.BadRequest("Invited user is already a group member.");
            }

            User? user = await this.db.Users!.Where(u => u.Id == invite.InvitedUserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return this.BadRequest("Invited user does not exist.");
            }
            else if (user.AcceptGroupInvitations == false)
            {
                return this.BadRequest("User not accepting invitations");
            }

            try
            {
                EntityEntry<MessageGroupInvitation> newInvitation = await this.db.MessageGroupInvitations!.AddAsync(invite);
                int completed = await this.db.SaveChangesAsync();
                if (completed != 1)
                {
                    return this.BadRequest("Failed to create invitation.");
                }

                return this.CreatedAtRoute(
                routeName: nameof(this.GetMessageGroupInvitation),
                routeValues: new { id = newInvitation.Entity.Id },
                value: newInvitation.Entity);
            }
            catch (Exception)
            {
                // Log this exception
                return this.BadRequest("Error when creating invitation.");
            }
        }

        /// <summary>
        /// PATCH: api/messagegroupinvitation/decline/[id]; <br />
        /// Decline specified message group invitation. Must be the invitee.
        /// </summary>
        /// <param name="inviteId">Id of the specified message group invitation.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpPatch("decline/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeclineMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return this.BadRequest("Invalid invite id.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            MessageGroupInvitation? mgi = await this.db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return this.NotFound();
            }
            else if (mgi.InvitationActive == false)
            {
                return this.BadRequest("Invitation already used.");
            }
            else if (claimId != mgi.InvitedUserId)
            {
                return this.Unauthorized("Not invite recipient.");
            }

            try
            {
                mgi.InvitationActive = false;
                mgi.InvitationAccepted = false;
                mgi.ActionDate = DateTime.Now;

                this.db.MessageGroupInvitations!.Update(mgi);
                int completed = await this.db.SaveChangesAsync();

                if (completed == 1)
                {
                    return this.NoContent();
                }
                else
                {
                    return this.BadRequest("Failed to decline invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                return this.BadRequest("Error when declining invitation.");
            }
        }

        /// <summary>
        /// PATCH: api/messagegroupinvitation/accept/[id]; <br />
        /// Accept specified message group invitation. Must be the invitee.
        /// </summary>
        /// <param name="inviteId">Id of the specified message group invitation.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpPatch("accept/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AcceptMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return this.BadRequest("Invalid invite id.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            MessageGroupInvitation? mgi = await this.db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return this.NotFound();
            }
            else if (mgi.InvitationActive == false)
            {
                return this.BadRequest("Invitation already used.");
            }
            else if (claimId != mgi.InvitedUserId)
            {
                return this.Unauthorized("Not invite recipient.");
            }

            using var transaction = this.db.Database.BeginTransaction();
            try
            {
                mgi.InvitationActive = false;
                mgi.InvitationAccepted = true;
                mgi.ActionDate = DateTime.Now;

                this.db.MessageGroupInvitations!.Update(mgi);

                int completed1 = await this.db.SaveChangesAsync();

                MessageGroupMember mgm = new ()
                {
                    MessageGroupId = mgi.MessageGroupId,
                    MessageGroupRoleId = 2,
                    UserId = mgi.InvitedUserId,
                };

                EntityEntry<MessageGroupMember> newMGM = await this.db.MessageGroupMembers!.AddAsync(mgm);

                int completed2 = await this.db.SaveChangesAsync();

                if (completed1 == 1 && completed2 == 1)
                {
                    transaction.Commit();
                    return this.NoContent();
                }
                else
                {
                    transaction.Rollback();
                    return this.BadRequest("Failed to accept invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                transaction.Rollback();
                return this.BadRequest("Error when accepting invitation.");
            }
        }

        /// <summary>
        /// DELETE: api/messagegroupinvitation/delete/[id]; <br />
        /// Delete the specified message group invitation. Must be the requester of the invite.
        /// </summary>
        /// <param name="inviteId">Id of the specified message group invite.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpDelete("delete/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return this.BadRequest("Invalid invite id.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            MessageGroupInvitation? mgi = await this.db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return this.NotFound();
            }
            else if (claimId != mgi.InvitingUserId)
            {
                return this.Unauthorized("Not invite sender.");
            }

            try
            {
                this.db.MessageGroupInvitations!.Remove(mgi);
                int completed = await this.db.SaveChangesAsync();

                if (completed == 1)
                {
                    return this.NoContent();
                }
                else
                {
                    return this.BadRequest("Failed to delete invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                return this.BadRequest("Error when deleting invitation.");
            }
        }
    }
}
