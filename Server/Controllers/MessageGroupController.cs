using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using HearYe.Shared;
using Microsoft.EntityFrameworkCore;
using HearYe.Server.Helpers;

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
            MessageGroup? messageGroup = await db.MessageGroups!.Where(mg => mg.Id == id).FirstOrDefaultAsync();

            if (messageGroup == null)
            {
                return NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, id);

            if (roleId == 0)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, id);

            if (roleId == 0)
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
            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);

            if (claimId == 0)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(groupName))
            {
                return BadRequest("New group name required.");
            }

            if (groupName.Length > 50 || groupName.Length < 4)
            {
                return BadRequest("New group name must be between 4 and 50 characters.");
            }

            using var transaction = db.Database.BeginTransaction();

            try
            {
                // Make group
                MessageGroup newGroup = new MessageGroup()
                {
                    MessageGroupName = groupName,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                };
                EntityEntry<MessageGroup> newGroupEntry = await db.MessageGroups!.AddAsync(newGroup);

                int completedGroup = await db.SaveChangesAsync();
                if (completedGroup != 1)
                {
                    transaction.Rollback();
                    return BadRequest("Failed to create new message group.");
                }

                // Assign user requesting new group as group admin
                MessageGroupMember newGroupAdmin = new MessageGroupMember()
                {
                    MessageGroupId = newGroupEntry.Entity.Id,
                    MessageGroupRoleId = 1, // Admin
                    UserId = claimId
                };
                EntityEntry<MessageGroupMember> newGroupAdminEntry = await db.MessageGroupMembers!.AddAsync(newGroupAdmin);

                int completedAdmin = await db.SaveChangesAsync();
                if (completedAdmin != 1)
                {
                    transaction.Rollback();
                    return BadRequest("Failed to add users to group.");
                }

                transaction.Commit();

                return CreatedAtRoute(
                    routeName: nameof(GetMessageGroup),
                    routeValues: new { id = newGroupEntry.Entity.Id },
                    value: newGroupEntry.Entity);
            }
            catch (Exception)
            {
                // Log this exception.
                transaction.Rollback();
                return BadRequest("Failed to create new message group.");
            }
        }

        // PUT: api/messagegroup/setrole
        // BODY: MessageGroupMember (JSON)
        [HttpPut("setrole")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetMessageGroupRole([FromBody] MessageGroupMember mgm)
        {
            if (mgm == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, mgm.MessageGroupId);

            if (roleId != 1)
            {
                return Unauthorized();
            }

            using var transaction = db.Database.BeginTransaction();
            try
            {
                EntityEntry<MessageGroupMember> mgmUpdate = db.MessageGroupMembers!.Update(mgm);

                if (mgmUpdate.State == EntityState.Added)
                {
                    transaction.Rollback();
                    return BadRequest("Specified user is not group member.");
                }

                int completed = await db.SaveChangesAsync();

                if (completed != 1)
                {
                    transaction.Rollback();
                    return BadRequest("Failed to set message group member role.");
                }

                transaction.Commit();
                return new NoContentResult();
            }
            catch (Exception)
            {
                // Log this exception.
                transaction.Rollback();
                return BadRequest("Error when setting message group member role.");
            }
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
            MessageGroup? mg = await db.MessageGroups!.Where(mg => mg.Id == id).FirstOrDefaultAsync();

            if (mg == null || id == 0)
            {
                return NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, id);

            if (roleId != 1)
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
    }
}
