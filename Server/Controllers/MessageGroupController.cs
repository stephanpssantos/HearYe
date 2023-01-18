using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using HearYe.Shared;
using Microsoft.EntityFrameworkCore;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class MessageGroupController : ControllerBase
    {
        private readonly HearYeContext db;

        public MessageGroupController(HearYeContext db)
        {
            this.db = db;
        }

        // GET: api/messagegroup/[id]
        [HttpGet("{id:int}", Name = nameof(GetMessageGroup))]
        [ProducesResponseType(200, Type = typeof(MessageGroup))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroup(int id)
        {
            MessageGroup? messageGroup = await db.MessageGroups!.Where(mg => mg.Id == id).FirstAsync();

            if (messageGroup == null)
            {
                return NotFound();
            }

            int authCheck = await UserAuthCheck(HttpContext.User.Claims, id);
            if (authCheck == -1)
            {
                return Unauthorized();
            }

            return Ok(messageGroup);
        }

        // GET: api/messagegroup/members/[id]
        [HttpGet("members/{id:int}", Name = nameof(GetMessageGroupMembers))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroupMember>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMessageGroupMembers(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            int authCheck = await UserAuthCheck(HttpContext.User.Claims, id);
            if (authCheck == -1)
            {
                return Unauthorized();
            }

            IEnumerable<MessageGroupMember?> mg = await db.MessageGroupMembers!
                .Where(mgm => mgm.MessageGroupId == id)
                .Include(mgm => mgm.User)
                .ToListAsync();

            return Ok(mg);
        }

        // POST: api/messagegroup/new
        // BODY: string
        [HttpPost("new")]
        [ProducesResponseType(201, Type = typeof(MessageGroup))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewMessageGroup([FromBody] string groupName)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool parseClaimId = int.TryParse(claimId, out int claimIdInt);

            if (claimId == null || !parseClaimId)
            {
                return Unauthorized();
            }

            if (groupName == null)
            {
                return BadRequest("New group name required.");
            }

            // Make group
            MessageGroup newGroup = new MessageGroup()
            {
                MessageGroupName = groupName,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
            };
            EntityEntry<MessageGroup> newGroupEntry = await db.MessageGroups!.AddAsync(newGroup);

            // Assign user requesting new group as group admin
            MessageGroupMember newGroupAdmin = new MessageGroupMember()
            {
                MessageGroupId = newGroupEntry.Entity.Id,
                MessageGroupRoleId = 1, // Admin
                UserId = claimIdInt
            };
            EntityEntry<MessageGroupMember> newGroupAdminEntry = await db.MessageGroupMembers!.AddAsync(newGroupAdmin);

            int completed = await db.SaveChangesAsync();
            if (completed != 1)
            {
                return BadRequest("Failed to create new message group.");
            }

            return CreatedAtRoute(
                routeName: nameof(GetMessageGroup),
                routeValues: new { id = newGroupEntry.Entity.Id },
                value: newGroupEntry.Entity);
        }

        // PUT: api/messagegroup/setrole
        // BODY: MessageGroupMember (JSON)
        [HttpPut("setrole")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetMessageGroupRole([FromBody] MessageGroupMember mgm)
        {
            if (mgm == null)
            {
                return BadRequest();
            }

            int authCheck = await UserAuthCheck(HttpContext.User.Claims, mgm.MessageGroupId);
            if (authCheck != 1) // if user does not have group admin role
            {
                return Unauthorized();
            }

            db.MessageGroupMembers!.Update(mgm);
            int completed = await db.SaveChangesAsync();

            if (completed != 1)
            {
                return BadRequest("Failed to set message group member role.");
            }

            return new NoContentResult();
        }

        // DELETE: api/messagegroup/[id]
        // Soft delete.
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessageGroup(int id)
        {
            MessageGroup mg = await db.MessageGroups!.Where(mg => mg.Id == id).FirstAsync();

            if (mg == null || id == 0)
            {
                return NotFound();
            }

            int authCheck = await UserAuthCheck(HttpContext.User.Claims, id!);

            if (authCheck != 1) // If requester is not message group admin
            {
                return Unauthorized();
            }

            mg.IsDeleted = true;
            mg.DeletedDate = DateTime.Now;
            db.MessageGroups!.Update(mg);

            int completed = await db.SaveChangesAsync();

            if (completed == 1)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Message group found but failed to delete.");
            }
        }

        /// <summary>
        /// Checks that the user is logged in and belongs to the MessageGroup being requested.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <param name="messageGroupId">MessageGroup.Id of specified group.</param>
        /// <returns>
        /// An int with the user's role id within the specified group. If the user
        /// is not authorized (not a member or not assigned a role) returns -1.
        /// </returns>
        private async Task<int> UserAuthCheck(IEnumerable<Claim> claims, int messageGroupId)
        {
            string? claimId = claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool success = int.TryParse(claimId, out int claimIdInt);

            if (claimId == null || messageGroupId == 0 || !success)
            {
                return -1;
            }

            MessageGroupMember? mgm = await db.MessageGroupMembers!
                .Where(mgm => mgm.UserId == claimIdInt && mgm.MessageGroupId == messageGroupId)
                .FirstAsync();

            return mgm != null ? mgm.MessageGroupRoleId ?? -1 : -1;
        }
    }
}
