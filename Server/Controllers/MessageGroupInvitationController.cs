using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using HearYe.Shared;
using HearYe.Server.Helpers;

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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserInviteAuthCheck(db, claimId, id);

            if (roleId == 0)
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
            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0 || userId != claimId)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, invite.MessageGroupId);

            if (roleId == 0)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0)
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
            else if (claimId != mgi.InvitedUserId)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0)
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
            else if (claimId != mgi.InvitedUserId)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0)
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
            else if (claimId != mgi.InvitingUserId)
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
    }
}
