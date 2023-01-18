using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using HearYe.Shared;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class PostController : ControllerBase
    {
        private readonly HearYeContext db;

        public PostController(HearYeContext db)
        {
            this.db = db;
        }

        // GET: api/post/[id]
        [HttpGet("{id:int}", Name = nameof(GetPost))]
        [ProducesResponseType(200, Type = typeof(Post))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPost(int id)
        {
            Post? post = await db.Posts!.Where(p => p.Id == id).FirstAsync();

            if (post == null)
            {
                return NotFound();
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)post.MessageGroupId!);

            if (userId == -1)
            {
                return Unauthorized();
            }

            return Ok(post);
        }

        // GET: api/post/new?messageGroupId=[messageGroupId]&count=[count]&skip=[skip]
        [HttpGet("new")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Post>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetNewPosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return BadRequest();
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)messageGroupId);

            if (userId == -1)
            {
                return Unauthorized();
            }

            IEnumerable<Post?> posts = await db.Posts!
                .Include("Acknowledgements")
                .Where
                (
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTime.Now)
                    && (p.Acknowledgements!.Where(a => a.UserId == userId)).All(a => p.Id != a.PostId)
                )
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/post/acknowledged?messageGroupId=[messageGroupId]&count=[count]&skip=[skip]
        [HttpGet("acknowledged")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Post>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAcknowledgedPosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return BadRequest();
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)messageGroupId);

            if (userId == -1)
            {
                return Unauthorized();
            }

            IEnumerable<Post?> posts = await db.Posts!
                .Include("Acknowledgements")
                .Where
                (
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTime.Now)
                    && (p.Acknowledgements!.Where(a => a.UserId == userId)).All(a => p.Id == a.PostId)
                )
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/post/stale?messageGroupId=[messageGroupId]&count=[count]&skip=[skip]
        [HttpGet("stale")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Post>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStalePosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return BadRequest();
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)messageGroupId);

            if (userId == -1)
            {
                return Unauthorized();
            }

            IEnumerable<Post?> posts = await db.Posts!
                .Include("Acknowledgements")
                .Where(p => p.MessageGroupId == messageGroupId && p.StaleDate <= DateTime.Now)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/post
        // BODY: Post (JSON)
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewPost([FromBody] Post post)
        {
            if (post == null || post.MessageGroupId == null)
            {
                return BadRequest("Complete post object required.");
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)post.MessageGroupId!);

            if (userId == -1)
            {
                return Unauthorized();
            }

            EntityEntry<Post> newPost = await db.Posts!.AddAsync(post);
            int completed = await db.SaveChangesAsync();

            if (completed != 1)
            {
                return BadRequest("Failed to create new post.");
            }

            return CreatedAtRoute(
                routeName: nameof(GetPost),
                routeValues: new { id = newPost.Entity.Id },
                value: newPost.Entity);
        }

        // DELETE: api/post/[id]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePost(int id)
        {
            Post post = await db.Posts!.Where(p => p.Id == id).FirstAsync();

            if (post == null || post.MessageGroupId == null)
            {
                return NotFound();
            }

            int userId = await UserAuthCheck(HttpContext.User.Claims, (int)post.MessageGroupId!);

            if (userId == -1 || userId != post.UserId)
            {
                return Unauthorized();
            }

            db.Posts!.Remove(post);
            int completed = await db.SaveChangesAsync();

            if (completed == 1)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Post found but failed to delete.");
            }
        }

        /// <summary>
        /// Checks that the user is logged in and belongs to the MessageGroup being requested.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <param name="messageGroupId">MessageGroup.Id of specified group.</param>
        /// <returns>
        /// An int with the authorized user's database Id or a -1 indicating that the 
        /// user is not authorized to access the specified MessageGroup.
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

            return mgm != null ? claimIdInt : -1;
        }
    }
}
