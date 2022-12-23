using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using HearYe.Shared;
using User = HearYe.Shared.User;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class UserController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly GraphServiceClient graph;

        public UserController(HearYeContext db, GraphServiceClient graph) 
        { 
            this.db = db;
            this.graph = graph;
        }

        // GET: api/user/[id]
        [HttpGet("{id:int}", Name = nameof(GetUser))]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(int id)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;

            if (claimId == null || claimId != id.ToString()) 
            {
                return Unauthorized();
            }

            User? user = await db.Users!.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/user/?aadOid=[aadOid]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByOid(string aadOid)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;

            if (claimId == null)
            {
                return Unauthorized();
            }

            Guid input = new (aadOid);
            User? user = await db.Users!
                .Where(u => u.AadOid == input)
                .FirstAsync();

            if (user == null)
            {
                return NotFound();
            }
            else if (user.Id.ToString() != claimId)
            {
                return Unauthorized();
            }
            return Ok(user);
        }

        // GET: api/user/groups/[id]
        [HttpGet("groups/{id:int}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroup>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserMessageGroups(int id)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;

            if (claimId == null || claimId != id.ToString())
            {
                return Unauthorized();
            }

            IEnumerable<MessageGroup?> mg = await db.MessageGroupMembers!
                .Where(mgm => mgm.UserId == id)
                .Select(mgm => mgm.MessageGroup)
                .ToListAsync();

            return Ok(mg);
        }

        // POST: api/user
        // BODY: User (JSON)
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> NewUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User object required.");
            }

            string? aadOid = HttpContext.User.Claims.FirstOrDefault(x => 
                x.Type.Equals("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            if (aadOid == null || aadOid != user.AadOid.ToString())
            {
                return BadRequest("Azure AD object ID (AadOid) missing or mismatched.");
            }

            EntityEntry<User> newUser = await db.Users!.AddAsync(user);
            int completed = await db.SaveChangesAsync();

            if (completed != 1)
            {
                return BadRequest("Failed to create new user.");
            }

            try
            {
                Microsoft.Graph.User newGraphUser = new()
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "extension_9ad29a8ab7fc468aa9c975e45b6eb34e_DatabaseId", newUser.Entity.Id.ToString() }
                    }
                };

                await graph.Users[aadOid].Request().UpdateAsync(newGraphUser);

                return CreatedAtRoute(
                    routeName: nameof(GetUser),
                    routeValues: new { id = newUser.Entity.Id },
                    value: newUser.Entity);
            }
            catch (Exception)
            {
                // Log this exception.
                return BadRequest();
            }
        }

        // PUT: api/user
        // BODY: User (JSON)
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;

            if (user == null || user.Id != id)
            {
                return BadRequest();
            }

            if (claimId == null || claimId != id.ToString())
            {
                return Unauthorized();
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

        // DELETE: api/user/[id]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            string? claimId = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;

            if (claimId == null || claimId != id.ToString())
            {
                return Unauthorized();
            }

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
