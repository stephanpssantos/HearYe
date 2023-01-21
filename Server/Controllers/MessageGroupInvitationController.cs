using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;
using System.ComponentModel;
using System.Security.Claims;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class MessageGroupInvitationController : ControllerBase
    {
        private readonly HearYeContext db;

        public MessageGroupInvitationController(HearYeContext db)
        {
            this.db = db;
        }

        // GET: api/messagegroupinvitation/[id]
        [HttpGet("{id:int}", Name = nameof(GetMessageGroupInvitation))]
        [ProducesResponseType(200, Type = typeof(MessageGroupInvitation))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroupInvitation(int id)
        {
            MessageGroupInvitation? messageGroupInvitation = await db.MessageGroupInvitations!.Where(mgi => mgi.Id == id).FirstOrDefaultAsync();

            if (messageGroupInvitation == null)
            {
                return NotFound();
            }

            int authCheck = await UserInviteAuthCheck(HttpContext.User.Claims, id);
            if (authCheck == 0)
            {
                return Unauthorized();
            }

            return Ok(messageGroupInvitation);
        }

        // GET: api/messagegroupinvitations/[id]
        [HttpGet("{id:int}", Name = nameof(GetMessageGroupInvitations))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroupInvitation>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMessageGroupInvitations(int userId)
        {
            int authCheck = UserClaimCheck(HttpContext.User.Claims);
            if (authCheck == 0 || userId != authCheck)
            {
                return Unauthorized();
            }

            IEnumerable<MessageGroupInvitation> messageGroupInvitations = await db.MessageGroupInvitations!
                .Where(mgi => (mgi.InvitationActive == true) 
                    && (mgi.InvitingUserId == userId) 
                    || (mgi.InvitedUserId == userId))
                .ToListAsync();

            return Ok(messageGroupInvitations);
        }

        // POST: api/messagegroupinvitations/new
        // BODY: string
        [HttpPost("new")]
        [ProducesResponseType(201, Type = typeof(MessageGroupInvitation))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewMessageGroupInvitation([FromBody] MessageGroupInvitation invite)
        {
            if (invite == null || !ModelState.IsValid || invite.InvitationAccepted == true || invite.InvitationActive == false)
            {
                return BadRequest();
            }

            int claimCheck = await UserGroupAuthCheck(HttpContext.User.Claims, invite.MessageGroupId);
            if (claimCheck == 0)
            {
                return Unauthorized();
            }

            MessageGroupMember? invitee = await db.MessageGroupMembers!
                .Where(members => (members.MessageGroupId == invite.MessageGroupId) && (members.UserId == invite.InvitedUserId))
                .FirstOrDefaultAsync();

            if (invitee != null)
            {
                return BadRequest("Invited user is already a group member.");
            }

            User? user = await db.Users!.Where(u => u.Id == invite.InvitedUserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest("Invited user does not exist.");
            }
            if (user.AcceptGroupInvitations == false)
            {
                return BadRequest("User not accepting invitations");
            }

            try
            {
                EntityEntry<MessageGroupInvitation> newInvitation = await db.MessageGroupInvitations!.AddAsync(invite);
                int completed = await db.SaveChangesAsync();
                if (completed != 1)
                {
                    return BadRequest("Failed to create invitation.");
                }

                return CreatedAtRoute(
                routeName: nameof(GetMessageGroupInvitation),
                routeValues: new { id = newInvitation.Entity.Id },
                value: newInvitation.Entity);
            }
            catch (Exception)
            {
                // Log this exception
                return BadRequest("Error when creating invitation.");
            }
        }

        // PATCH: api/messagegroupinvitation/decline/[id]
        [HttpPatch("decline/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeclineMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return BadRequest("Invalid invite id.");
            }

            int claimCheck = UserClaimCheck(HttpContext.User.Claims);
            if (claimCheck == 0)
            {
                return Unauthorized();
            }

            MessageGroupInvitation? mgi = await db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return NotFound();
            }
            else if (mgi.InvitationActive == false)
            {
                return BadRequest("Invitation already used.");
            }
            else if (claimCheck != mgi.InvitedUserId)
            {
                return Unauthorized("Not invite recipient.");
            }

            try
            {
                mgi.InvitationActive = false;
                mgi.InvitationAccepted = false;
                mgi.ActionDate = DateTime.Now;

                db.MessageGroupInvitations!.Update(mgi);
                int completed = await db.SaveChangesAsync();

                if (completed == 1)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Failed to decline invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                return BadRequest("Error when declining invitation.");
            }
        }

        // PATCH: api/messagegroupinvitation/accept/[id]
        [HttpPatch("accept/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AcceptMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return BadRequest("Invalid invite id.");
            }

            int claimCheck = UserClaimCheck(HttpContext.User.Claims);
            if (claimCheck == 0)
            {
                return Unauthorized();
            }

            MessageGroupInvitation? mgi = await db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return NotFound();
            }
            else if (mgi.InvitationActive == false)
            {
                return BadRequest("Invitation already used.");
            }
            else if (claimCheck != mgi.InvitedUserId)
            {
                return Unauthorized("Not invite recipient.");
            }

            using var transaction = db.Database.BeginTransaction();
            try
            {
                mgi.InvitationActive = false;
                mgi.InvitationAccepted = true;
                mgi.ActionDate = DateTime.Now;

                db.MessageGroupInvitations!.Update(mgi);

                int completed1 = await db.SaveChangesAsync();

                MessageGroupMember mgm = new()
                {
                    MessageGroupId = mgi.MessageGroupId,
                    MessageGroupRoleId = 2,
                    UserId = mgi.InvitedUserId
                };

                EntityEntry<MessageGroupMember> newMGM = await db.MessageGroupMembers!.AddAsync(mgm);

                int completed2 = await db.SaveChangesAsync();

                if (completed1 == 1 && completed2 == 1)
                {
                    transaction.Commit();
                    return NoContent();
                }
                else
                {
                    transaction.Rollback();
                    return BadRequest("Failed to accept invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                transaction.Rollback();
                return BadRequest("Error when accepting invitation.");
            }
        }

        // DELETE: api/messagegroupinvitation/delete/[id]
        [HttpDelete("delete/{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroupInvitation(int inviteId)
        {
            if (inviteId < 1)
            {
                return BadRequest("Invalid invite id.");
            }

            int claimCheck = UserClaimCheck(HttpContext.User.Claims);
            if (claimCheck == 0)
            {
                return Unauthorized();
            }

            MessageGroupInvitation? mgi = await db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId)
                .FirstOrDefaultAsync();

            if (mgi == null)
            {
                return NotFound();
            }
            else if (claimCheck != mgi.InvitingUserId)
            {
                return Unauthorized("Not invite sender.");
            }

            try
            {
                db.MessageGroupInvitations!.Remove(mgi);
                int completed = await db.SaveChangesAsync();

                if (completed == 1)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Failed to delete invitation.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                return BadRequest("Error when deleting invitation.");
            }
        }

        /// <summary>
        /// Checks that the user is logged in finds their database ID within their claims.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <returns>
        /// The user's database ID or 0 if the user is not logged in or does not have the correct claims.
        /// </returns>
        private int UserClaimCheck(IEnumerable<Claim> claims)
        {
            string? claimId = claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool success = int.TryParse(claimId, out int claimIdInt);

            if (claimId == null || !success)
            {
                return 0;
            }

            return claimIdInt;
        }

        /// <summary>
        /// Checks that the user is logged in and is involved with the invite being requested.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <param name="inviteId">Invite.Id of specified invite.</param>
        /// <returns>
        /// 0 if the user is not involved in the invite.
        /// 1 if the user is the inviting user of the specified invite.
        /// 2 if the user is the invited user of the specified invite.
        /// </returns>
        private async Task<int> UserInviteAuthCheck(IEnumerable<Claim> claims, int inviteId)
        {
            int claimCheck = UserClaimCheck(claims);

            if (claimCheck == 0 || inviteId == 0)
            {
                return 0;
            }

            MessageGroupInvitation? mgi = await db.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId 
                    && (inv.InvitedUserId == claimCheck || inv.InvitingUserId == claimCheck))
                .FirstOrDefaultAsync();

            if (mgi is not null && mgi.InvitingUserId == claimCheck)
            {
                return 1;
            }

            if (mgi is not null && mgi.InvitedUserId == claimCheck)
            {
                return 2;
            }

            return 0;
        }

        /// <summary>
        /// Checks that the user is logged in and has a role in the specified group.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <param name="groupId">Group.Id of the requested group.</param>
        /// <returns>
        /// 0 if the user is not a group member.
        /// 1 if the user is a group admin.
        /// 2 if the user is a group member.
        /// </returns>
        private async Task<int> UserGroupAuthCheck(IEnumerable<Claim> claims, int groupId)
        {
            int claimCheck = UserClaimCheck(claims);

            if (claimCheck == 0 || groupId == 0)
            {
                return 0;
            }

            MessageGroupMember? mgm = await db.MessageGroupMembers!
                .Where(members => members.MessageGroupId == groupId && members.UserId == claimCheck)
                .FirstOrDefaultAsync();

            if (mgm is null || mgm.MessageGroupRoleId is null) 
            { 
                return 0; 
            }
            else
            {
                return (int)mgm.MessageGroupRoleId;
            }
        }
    }
}
