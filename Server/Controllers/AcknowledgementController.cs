﻿// <copyright file="AcknowledgementController.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;

namespace HearYe.Server.Controllers
{
    /// <summary>
    /// Handles requests related to post acknowledgements.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class AcknowledgementController : ControllerBase
    {
        private readonly HearYeContext db;
        private readonly ILogger<AcknowledgementController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcknowledgementController"/> class.
        /// </summary>
        /// <param name="db">HearYeContext instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public AcknowledgementController(HearYeContext db, ILogger<AcknowledgementController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// POST: api/acknowledgement; <br />
        /// Create new acknowledgement record.
        /// </summary>
        /// <param name="acknowledgement">Acknowledgement object included in request body in JSON format.</param>
        /// <returns>204, 400, or 401.</returns>
        [HttpPost(Name = nameof(NewAcknowledgement))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewAcknowledgement([FromBody] Acknowledgement acknowledgement)
        {
            if (acknowledgement == null || !this.ModelState.IsValid)
            {
                return this.BadRequest("Complete acknowledgement object required.");
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0 || acknowledgement.UserId != claimId)
            {
                return this.Unauthorized();
            }

            Post? postCheck = await this.db.Posts!
                .Where(post => post.Id == acknowledgement.PostId)
                .FirstOrDefaultAsync();

            if (postCheck == null || postCheck.MessageGroupId == null)
            {
                return this.BadRequest();
            }

            int membershipCheck = await AuthCheck.UserGroupAuthCheck(this.db, claimId, (int)postCheck.MessageGroupId);

            if (membershipCheck == 0)
            {
                return this.Unauthorized();
            }

            try
            {
                EntityEntry<Acknowledgement> newAcknowledgement = await this.db.Acknowledgements!.AddAsync(acknowledgement);
                int completed = await this.db.SaveChangesAsync();

                if (completed != 1)
                {
                    this.logger.LogError("New acknowledgement not saved.");
                    this.logger.LogError("Failure: {failedAcknowledgement}", JsonSerializer.Serialize(acknowledgement, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to create new acknowledgement.");
                }

                return this.NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error when creating new acknowledgement.");
                this.logger.LogError("Exception: {exceptionMessage}", ex.Message);
                this.logger.LogError("Failure: {failedAcknowledgement}", JsonSerializer.Serialize(acknowledgement, CustomJsonOptions.IgnoreCycles()));
                return this.BadRequest("Error when creating new acknowledgement.");
            }
        }

        /// <summary>
        /// DELETE: api/acknowledgement/delete?postId=[postId]&amp;userId=[userId]; <br />
        /// Delete specified post acknowledgement.
        /// </summary>
        /// <param name="postId">Post ID of acknowledgement to delete.</param>
        /// <param name="userId">User ID of acknowledgement to delete.</param>
        /// <returns>204, 400, 401, or 404.</returns>
        [HttpDelete("delete", Name = nameof(DeleteAcknowledgement))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAcknowledgement(int postId, int userId)
        {
            if (postId < 1 || userId < 1)
            {
                return this.BadRequest();
            }

            int claimId = AuthCheck.UserClaimCheck(this.HttpContext.User.Claims);
            if (claimId == 0)
            {
                return this.Unauthorized();
            }

            Acknowledgement? acknowledgement = await this.db.Acknowledgements!
                .Where(a => a.PostId == postId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (acknowledgement == null)
            {
                return this.NotFound();
            }

            if (acknowledgement!.UserId != claimId)
            {
                return this.Unauthorized("Not poster of acknowledgement.");
            }

            try
            {
                this.db.Acknowledgements!.Remove(acknowledgement);
                int completed = await this.db.SaveChangesAsync();

                if (completed == 1)
                {
                    return this.NoContent();
                }
                else
                {
                    this.logger.LogError("Error when deleting acknowledgement.");
                    this.logger.LogError("Failure: {failedAcknowledgement}", JsonSerializer.Serialize(acknowledgement, CustomJsonOptions.IgnoreCycles()));
                    return this.BadRequest("Failed to delete acknowledgement.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error when deleting acknowledgement.");
                this.logger.LogError("Exception: {exceptionMessage}", ex.Message);
                this.logger.LogError("Failure: {failedAcknowledgement}", JsonSerializer.Serialize(acknowledgement, CustomJsonOptions.IgnoreCycles()));
                return this.BadRequest("Error when deleting acknowledgement.");
            }
        }
    }
}
