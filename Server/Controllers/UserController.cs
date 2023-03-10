// <copyright file="UserController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json;
using Azure.Identity;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync
using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Microsoft.Graph;
using Microsoft.Identity.Web.Resource;
using User = HearYe.Shared.User;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to users.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class UserController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly GraphServiceClient graph;
        private readonly ILogger<UserController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        /// <param name="graph">GraphServiceClient instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public UserController(HearYeContext db, GraphServiceClient graph, ILogger<UserController> logger)
        {
            this.db = db;
            this.graph = graph;
            this.logger = logger;
        }

        /// <summary>
        /// GET: api/user/[id]; <br />
        /// Get the specified user by id.
        /// </summary>
        /// <param name="id">Id of specified user.</param>
        /// <returns>200 (with user object in body), 401, or 404.</returns>
        [HttpGet("{id:int}", Name = nameof(GetUser))]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(int id)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0 || id != claimId)
            {
                return this.Unauthorized();
            }

            User? user = await this.db.Users!.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return this.NotFound();
            }

            return this.Ok(user);
        }

        /// <summary>
        /// GET: api/user/public/[id]; <br />
        /// Get public info about the specified user. Can be called even when not
        /// logged in as the specified user.
        /// </summary>
        /// <param name="id">Id of specified user.</param>
        /// <returns>200 (with user public info object in body), 401, or 404.</returns>
        [HttpGet("public/{id:int}", Name = nameof(GetUserPublicInfo))]
        [ProducesResponseType(200, Type = typeof(UserPublicInfo))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserPublicInfo(int id)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            User? user = await this.db.Users!.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return this.NotFound();
            }

            UserPublicInfo userPublic = new ()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                AcceptGroupInvitations = user.AcceptGroupInvitations,
            };

            return this.Ok(userPublic);
        }

        /// <summary>
        /// GET: api/user/?aadOid=[aadOid]; <br />
        /// Get user by Azure AD Object Id.
        /// Used for verifying new user claims (before the user's database ID is saved in their claims.)
        /// </summary>
        /// <param name="aadOid">User's Azure AD OID.</param>
        /// <returns>200 (with user object in body), 400, 401, or 404.</returns>
        [HttpGet(Name = nameof(GetUserByOid))]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByOid(string aadOid)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            bool validGuid = Guid.TryParse(aadOid, out Guid input);
            if (!validGuid)
            {
                return this.BadRequest();
            }

            User? user = await this.db.Users!
                .Where(u => u.AadOid == input)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return this.NotFound();
            }
            else if (user.Id != claimId)
            {
                return this.Unauthorized();
            }

            return this.Ok(user);
        }

        /// <summary>
        /// GET: api/user/groups/[id]; <br />
        /// Get a list of message groups that a user is a member of.
        /// </summary>
        /// <param name="id">Id of the specified user.</param>
        /// <returns>200 (with a list of message group objects in body), or 401.</returns>
        [HttpGet("groups/{id:int}", Name = nameof(GetUserMessageGroups))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<MessageGroup>))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserMessageGroups(int id)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0 || id != claimId)
            {
                return this.Unauthorized();
            }

            IEnumerable<MessageGroup?> mg = await this.db.MessageGroupMembers!
                .Where(mgm => mgm.UserId == id)
                .Select(mgm => mgm.MessageGroup)
                .ToListAsync();

            return this.Ok(mg);
        }

        /// <summary>
        /// POST: api/user; <br />
        /// Create new user then register new user id in Azure AD graph.
        /// The custom attribute 'extension_database' will be added to the user graph object.
        /// </summary>
        /// <param name="user">User object included in request body in JSON format.</param>
        /// <returns>201, 400, or 401.</returns>
        [HttpPost(Name = nameof(NewUser))]
        [ProducesResponseType(201, Type = typeof(User))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewUser([FromBody] User user)
        {
            if (user == null || !this.ModelState.IsValid)
            {
                return this.BadRequest("Valid user object required.");
            }

            string? aadOid = this.HttpContext.User.Claims.FirstOrDefault(x =>
                x.Type.Equals("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            if (aadOid == null)
            {
                return this.Unauthorized();
            }

            if (aadOid != user.AadOid.ToString())
            {
                return this.BadRequest("Azure AD object ID (AadOid) missing or mismatched.");
            }

            using var transaction = this.db.Database.BeginTransaction();
            try
            {
                EntityEntry<User> newUser = await this.db.Users!.AddAsync(user);

                try
                {
                    int completed = await this.db.SaveChangesAsync();
                    if (completed != 1)
                    {
                        throw new Exception("Could not save new user.");
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex.Message);
                    this.logger.LogError(ex.InnerException?.Message);
                    throw new Exception("Error when creating new user.");
                }

                Microsoft.Graph.User newGraphUser = new ()
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "extension_9ad29a8ab7fc468aa9c975e45b6eb34e_DatabaseId", newUser.Entity.Id.ToString() },
                    },
                };

                try
                {
                    // Sometimes transient errors occur here. If it's happening, try doing:
                    // this.graph.Users[aadOid].Request().WithMaxRetry(MaxRetry).WithShoudRetry(Func <in T, out TResult>(T arg)).UpdateAsync;
                    // Look at the docs of WithMaxRetry and WithShouldRetry for more info.
                    await this.graph.Users[aadOid].Request().UpdateAsync(newGraphUser);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex.Message);
                    throw new Exception("Failed to register graph record for new user.");
                }

                try
                {
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex.Message);
                    throw new Exception("Error committing changes to database.");
                }

                return this.CreatedAtRoute(
                    routeName: nameof(this.GetUser),
                    routeValues: new { id = newUser.Entity.Id },
                    value: newUser.Entity);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                this.logger.LogError("Error when creating new user.");
                this.logger.LogError(ex.Message);
                this.logger.LogError(JsonSerializer.Serialize(user), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// PUT: api/user/[id]; <br />
        /// Update user properties.
        /// </summary>
        /// <param name="id">Id of the user to update.</param>
        /// <param name="user">User object included in request body in JSON format.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpPut("{id:int}", Name = nameof(UpdateUser))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (user == null || user.Id != id)
            {
                return this.BadRequest("User ID mismatch.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0 || id != claimId)
            {
                return this.Unauthorized();
            }

            User? existing = await this.db.Users!.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            {
                return this.NotFound();
            }

            // These are the only properties that should be modifiable.
            existing.DisplayName = user.DisplayName;
            existing.AcceptGroupInvitations = user.AcceptGroupInvitations;
            existing.LastModifiedDate = user.LastModifiedDate;
            existing.DefaultGroupId = user.DefaultGroupId;

            this.db.Users!.Update(existing);
            int completed = await this.db.SaveChangesAsync();

            if (completed == 1)
            {
                return this.NoContent();
            }
            else
            {
                this.logger.LogError("Error when updating user.");
                this.logger.LogError(JsonSerializer.Serialize(existing), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("Failed to update user.");
            }
        }

        /// <summary>
        /// DELETE: api/user/[id]; <br />
        /// Deletes user account. Deletes only from application database.
        /// Azure AD user object is not deleted.
        /// </summary>
        /// <param name="id">Id of the user to delete.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpDelete("{id:int}", Name = nameof(DeleteUser))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);

            if (claimId == 0 || id != claimId)
            {
                return this.Unauthorized();
            }

            User? existing = await this.db.Users!.Where(u => u.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            {
                return this.NotFound();
            }

            this.db.Users!.Remove(existing);
            int completed = await this.db.SaveChangesAsync();

            if (completed == 1)
            {
                return this.NoContent();
            }
            else
            {
                this.logger.LogError("Error when deleting user.");
                this.logger.LogError(JsonSerializer.Serialize(existing), CustomJsonOptions.IgnoreCycles());
                return this.BadRequest("User found but failed to delete.");
            }
        }
    }
}
