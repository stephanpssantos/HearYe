using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            Post? post = await db.Posts!.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, id);

            if (roleId == 0)
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)messageGroupId);

            if (roleId == 0)
            {
                return Unauthorized();
            }

            IEnumerable<Post?> posts = await db.Posts!
                .Include("Acknowledgements")
                .Where
                (
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTime.Now)
                    && (p.Acknowledgements!.All(a => a.UserId != claimId))
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)messageGroupId);

            if (roleId == 0)
            {
                return Unauthorized();
            }

            IEnumerable<Post?> posts = await db.Posts!
                .Include("Acknowledgements")
                .Where
                (
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTime.Now)
                    && (p.Acknowledgements!.Any(a => a.UserId == claimId))
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)messageGroupId);

            if (roleId == 0)
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
            if (post == null || post.MessageGroupId == null || !ModelState.IsValid)
            {
                return BadRequest("Complete post object required.");
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)post.MessageGroupId!);

            if (roleId == 0)
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
            Post? post = await db.Posts!.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null || post.MessageGroupId == null)
            {
                return NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)post.MessageGroupId!);

            if (roleId == 0 || claimId != post.UserId)
            {
                return Unauthorized();
            }

            post.IsDeleted = true;
            post.DeletedDate = DateTime.Now;
            db.Posts!.Update(post);

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
    }
}
