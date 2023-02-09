// <copyright file="PostController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.Identity.Web.Resource;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to posts.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class PostController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly ILogger<PostController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public PostController(HearYeContext db, ILogger<PostController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// GET: api/post/[id]; <br />
        /// Get the specified post.
        /// </summary>
        /// <param name="id">Id of the specified post.</param>
        /// <returns>200 (with a post object), 401, or 400.</returns>
        [HttpGet("{id:int}", Name = nameof(GetPost))]
        [ProducesResponseType(200, Type = typeof(Post))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPost(int id)
        {
            Post? post = await this.db.Posts!.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null || post.MessageGroupId == null)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)post.MessageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            return this.Ok(post);
        }

        /// <summary>
        /// GET: api/post/acknowledged/[id]; <br />
        /// Get a list of users who have acknowledged a post.
        /// </summary>
        /// <param name="id">Id of the specified post.</param>
        /// <returns>200 (with a post object), 401, or 400.</returns>
        [HttpGet("acknowledged/{id:int}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<UserPublicInfo>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPostAcknowledgedUsers(int id)
        {
            Post? post = await this.db.Posts!.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null || post.MessageGroupId == null)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)post.MessageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            IEnumerable<UserPublicInfo> users = await this.db.Acknowledgements!
                .Include(a => a.User)
                .Where(a => a.PostId == id)
                .Select(a => new UserPublicInfo
                {
                    Id = a.UserId,
                    DisplayName = a.User == null ? "Unknown" : a.User.DisplayName,
                    AcceptGroupInvitations = a.User == null ? false : a.User.AcceptGroupInvitations,
                })
                .ToListAsync();

            return this.Ok(users);
        }

        /// <summary>
        /// GET: api/post/new?messageGroupId=[messageGroupId]&amp;count=[count]&amp;skip=[skip]; <br />
        /// Get posts that have not gone stale and that the requesting user has not acknowledged.
        /// </summary>
        /// <param name="messageGroupId">Id of a message group.</param>
        /// <param name="count">Number of posts to return.</param>
        /// <param name="skip">Number of posts to skip. Posts are ordered by creation date.</param>
        /// <returns>200 (with a list of post objects), 400, or 401.</returns>
        [HttpGet("new")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PostWithUserName>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetNewPosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)messageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            IEnumerable<PostWithUserName?> posts = await this.db.Posts!
                .Include("Acknowledgements")
                .Where(
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTimeOffset.Now)
                    && p.Acknowledgements!.All(a => a.UserId != claimId))
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .Select(p => new PostWithUserName
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    MessageGroupId = p.MessageGroupId,
                    Message = p.Message,
                    IsDeleted = p.IsDeleted,
                    CreatedDate = p.CreatedDate,
                    StaleDate = p.StaleDate,
                    DeletedDate = p.DeletedDate,
                    Acknowledgements = p.Acknowledgements,
                    DisplayName = p.User == null ? "Unknown" : p.User.DisplayName,
                })
                .ToListAsync();

            return this.Ok(posts);
        }

        /// <summary>
        /// GET: api/post/acknowledged?messageGroupId=[messageGroupId]&amp;count=[count]&amp;skip=[skip]; <br />
        /// Get posts that have not gone stale and that the requesting user has acknowledged.
        /// </summary>
        /// <param name="messageGroupId">Id of a message group.</param>
        /// <param name="count">Number of posts to return.</param>
        /// <param name="skip">Number of posts to skip. Posts are ordered by creation date.</param>
        /// <returns>200 (with a list of post objects), 400, or 401.</returns>
        [HttpGet("acknowledged")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PostWithUserName>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAcknowledgedPosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)messageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            IEnumerable<PostWithUserName?> posts = await this.db.Posts!
                .Include("Acknowledgements")
                .Where(
                    p => p.MessageGroupId == messageGroupId
                    && (p.StaleDate == null || p.StaleDate > DateTimeOffset.Now)
                    && p.Acknowledgements!.Any(a => a.UserId == claimId))
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .Select(p => new PostWithUserName
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    MessageGroupId = p.MessageGroupId,
                    Message = p.Message,
                    IsDeleted = p.IsDeleted,
                    CreatedDate = p.CreatedDate,
                    StaleDate = p.StaleDate,
                    DeletedDate = p.DeletedDate,
                    Acknowledgements = p.Acknowledgements,
                    DisplayName = p.User == null ? "Unknown" : p.User.DisplayName,
                })
                .ToListAsync();

            return this.Ok(posts);
        }

        /// <summary>
        /// GET: api/post/stale?messageGroupId=[messageGroupId]&amp;count=[count]&amp;skip=[skip]; <br />
        /// Get posts that have have gone stale.
        /// </summary>
        /// <param name="messageGroupId">Id of a message group.</param>
        /// <param name="count">Number of posts to return.</param>
        /// <param name="skip">Number of posts to skip. Posts are ordered by creation date.</param>
        /// <returns>200 (with a list of post objects), 400, or 401.</returns>
        [HttpGet("stale")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PostWithUserName>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStalePosts(int? messageGroupId, int count = 15, int skip = 0)
        {
            if (messageGroupId == null || messageGroupId == 0)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)messageGroupId);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            IEnumerable<PostWithUserName?> posts = await this.db.Posts!
                .Include("Acknowledgements")
                .Where(p => p.MessageGroupId == messageGroupId && p.StaleDate <= DateTimeOffset.Now)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(count)
                .Select(p => new PostWithUserName
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    MessageGroupId = p.MessageGroupId,
                    Message = p.Message,
                    IsDeleted = p.IsDeleted,
                    CreatedDate = p.CreatedDate,
                    StaleDate = p.StaleDate,
                    DeletedDate = p.DeletedDate,
                    Acknowledgements = p.Acknowledgements,
                    DisplayName = p.User == null ? "Unknown" : p.User.DisplayName,
                })
                .ToListAsync();

            return this.Ok(posts);
        }

        /// <summary>
        /// POST: api/post; <br />
        /// Create a new post.
        /// </summary>
        /// <param name="post">Post object included in request body in JSON format.</param>
        /// <returns>201, 400, or 404.</returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Post))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewPost([FromBody] Post post)
        {
            if (post == null || post.MessageGroupId == null || !this.ModelState.IsValid)
            {
                return this.BadRequest("Complete post object required.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)post.MessageGroupId!);

            if (roleId == 0)
            {
                return this.Unauthorized();
            }

            EntityEntry<Post> newPost = await this.db.Posts!.AddAsync(post);
            int completed = await this.db.SaveChangesAsync();

            if (completed != 1)
            {
                this.logger.LogError("New post transaction failed.");
                this.logger.LogError(JsonSerializer.Serialize(post), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Failed to create new post.");
            }

            return this.CreatedAtRoute(
                routeName: nameof(this.GetPost),
                routeValues: new { id = newPost.Entity.Id },
                value: newPost.Entity);
        }

        /// <summary>
        /// DELETE: api/post/[id]; <br />
        /// Delete the specified post.
        /// </summary>
        /// <param name="id">Id of the specified post.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePost(int id)
        {
            Post? post = await this.db.Posts!.Where(p => p.Id == id).FirstOrDefaultAsync();

            if (post == null || post.MessageGroupId == null)
            {
                return this.NotFound();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            int roleId = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)post.MessageGroupId!);

            if (roleId == 0 || claimId != post.UserId)
            {
                return this.Unauthorized();
            }

            post.IsDeleted = true;
            post.DeletedDate = DateTime.Now;
            this.db.Posts!.Update(post);

            int completed = await this.db.SaveChangesAsync();

            if (completed == 1)
            {
                return this.NoContent();
            }
            else
            {
                this.logger.LogError("Delete post transaction failed.");
                this.logger.LogError(JsonSerializer.Serialize(post), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Post found but failed to delete.");
            }
        }
    }
}
