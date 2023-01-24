using HearYe.Server.Helpers;
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0 || acknowledgement.UserId != claimId)
            {
                return Unauthorized();
            }

            Post? postCheck = await db.Posts!
                .Where(post => post.Id == acknowledgement.PostId)
                .FirstOrDefaultAsync();

            if (postCheck == null || postCheck.MessageGroupId == null)
            {
                return BadRequest();
            }

            int membershipCheck = await AuthCheck.UserGroupAuthCheck(db, claimId, (int)postCheck.MessageGroupId);

            if (membershipCheck == 0) 
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

            int claimId = AuthCheck.UserClaimCheck(HttpContext.User.Claims);
            if (claimId == 0)
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

            if (acknowledgement!.UserId != claimId)
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
    }
}
