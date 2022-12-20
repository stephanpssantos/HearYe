using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using HearYe.Shared;
using Microsoft.CodeAnalysis.Completion;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class UsersController : ControllerBase
    {
        private readonly HearYeContext db;

        public UsersController(HearYeContext db) 
        { 
            this.db = db;
        }

        // GET: api/users/[id]
        [HttpGet("{id:int}", Name = nameof(GetUserAsync))]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserAsync(int id)
        {
            User? user = await db.Users!.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // GET: api/users/?aadOid=[aadOid]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByOidAsync(string aadOid)
        {
            Guid input = new (aadOid);
            User? user = await db.Users!
                .Where(u => u.AadOid == input)
                .FirstAsync();

            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("groups/{id:int}")]
        [ProducesResponseType(200, Type = typeof(MessageGroup))]
        public async Task<IActionResult> GetUserMessageGroups(int id)
        {
            IEnumerable<MessageGroup?> mg = await db.MessageGroupMembers!
                .Where(mgm => mgm.UserId == id)
                .Select(mgm => mgm.MessageGroup)
                .ToListAsync();

            return Ok(mg);
        }

        // POST: api/users
        // BODY: User (JSON)
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> NewUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            EntityEntry<User> newUser = await db.Users!.AddAsync(user);
            int completed = await db.SaveChangesAsync();

            if (completed == 1)
            {
                return CreatedAtRoute(
                    routeName: nameof(GetUserAsync),
                    routeValues: new { id = newUser.Entity.Id },
                    value: newUser.Entity);
            }
            else
            {
                return BadRequest("Failed to create new user.");
            }
        }

        // PUT: api/users
        // BODY: User (JSON)
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (user == null || user.Id != id)
            {
                return BadRequest();
            }

            User? existing = await db.Users!.Where(u => u.Id == id).FirstAsync();
            if (existing == null)
            {
                return NotFound();
            }

            db.Users!.Update(user);
            int completed = await db.SaveChangesAsync();

            if (completed == 1)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to update user.");
            }
        }

        // DELETE: api/users/[id]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            User? existing = await db.Users!.Where(u => u.Id == id).FirstAsync();
            if (existing == null)
            {
                return NotFound();
            }

            db.Users!.Remove(existing);
            int completed = await db.SaveChangesAsync();

            if (completed == 1)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("User found but failed to delete.");
            }
        }
    }
}
