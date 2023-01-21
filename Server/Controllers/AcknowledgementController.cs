using HearYe.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;

namespace HearYe.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAdB2C:Scopes")]
    public class AcknowledgementController : ControllerBase
    {
        private readonly HearYeContext db;

        public AcknowledgementController(HearYeContext db)
        {
            this.db = db;
        }

        // POST: api/acknowledgement
        // BODY: Post (JSON)
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> NewAcknowledgement([FromBody] Acknowledgement acknowledgement)
        {
            if (acknowledgement == null || !ModelState.IsValid)
            {
                return BadRequest("Complete acknowledgement object required.");
            }

            int userId = UserClaimCheck(HttpContext.User.Claims);

            if (userId == 0 || acknowledgement.UserId != userId)
            {
                return Unauthorized();
            }

            Post? postCheck = await db.Posts!
                .Where(post => post.Id == acknowledgement.PostId)
                .FirstOrDefaultAsync();

            if (postCheck == null)
            {
                return BadRequest();
            }

            MessageGroupMember? membershipCheck = await db.MessageGroupMembers!
                .Where(mgm => mgm.UserId == userId && mgm.MessageGroupId == postCheck.MessageGroupId)
                .FirstOrDefaultAsync();

            if (membershipCheck == null || membershipCheck.MessageGroupRoleId == null) 
            {
                return Unauthorized();
            }

            try
            {
                EntityEntry<Acknowledgement> newAcknowledgement = await db.Acknowledgements!.AddAsync(acknowledgement);
                int completed = await db.SaveChangesAsync();

                if (completed != 1)
                {
                    return BadRequest("Failed to create new acknowledgement.");
                }

                return NoContent();
            }
            catch (Exception)
            {
                // Log this exception
                return BadRequest("Error when creating new acknowledgement.");
            }
            
        }

        // DELETE: api/acknowledgement/[id]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAcknowledgement(int acknowledgementId)
        {
            if (acknowledgementId < 1)
            {
                return BadRequest();
            }

            int userId = UserClaimCheck(HttpContext.User.Claims);

            if (userId == 0)
            {
                return Unauthorized();
            }

            Acknowledgement? acknowledgement = await db.Acknowledgements!
                .Where(a => a.Id == acknowledgementId)
                .FirstOrDefaultAsync();

            if (acknowledgement == null)
            {
                return NotFound();
            }

            if (acknowledgement!.UserId != userId)
            {
                return Unauthorized("Not poster of acknowledgement.");
            }

            try
            {
                db.Acknowledgements!.Remove(acknowledgement);
                int completed = await db.SaveChangesAsync();

                if (completed == 1)
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Failed to delete acknowledgement.");
                }
            }
            catch (Exception)
            {
                // Log this exception
                return BadRequest("Error when deleting acknowledgement.");
            }
        }

        /// <summary>
        /// Checks that the user is logged in finds their database ID within their claims.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <returns>
        /// The user's database ID or 0 if the user is not logged in or does not have the correct claims.
        /// </returns>
        private int UserClaimCheck(IEnumerable<Claim> claims)
        {
            string? claimId = claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool success = int.TryParse(claimId, out int claimIdInt);

            if (claimId == null || !success)
            {
                return 0;
            }

            return claimIdInt;
        }
    }
}
