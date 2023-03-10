// <copyright file="AuthCheck.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Security.Claims;
using HearYe.Shared;
using Microsoft.EntityFrameworkCore; // ToListAsync, FirstOrDefaultAsync

namespace HearYe.Server.Helpers
{
    /// <summary>
    /// Methods to verify user's authorization status. Checks auth token claims.
    /// </summary>
    public static class AuthCheck
    {
        /// <summary>
        /// Checks that the user is logged in finds their database ID within their claims.
        /// </summary>
        /// <param name="claims">HttpContext.User.Claims object.</param>
        /// <returns>
        /// The user's database ID or 0 if the user is not logged in or does not have the correct claims.
        /// </returns>
        public static int UserClaimCheck(IEnumerable<Claim> claims)
        {
            string? claimId = claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool success = int.TryParse(claimId, out int claimIdInt);

            if (claimId == null || !success)
            {
                return 0;
            }

            return claimIdInt;
        }

        /// <summary>
        /// Checks that the user belongs to the MessageGroup being requested.
        /// </summary>
        /// <param name="context">HearYeDatabaseContext.</param>
        /// <param name="userId">User id of user being checked.</param>
        /// <param name="messageGroupId">MessageGroup.Id of specified group.</param>
        /// <returns>
        /// An int with the user's role id within the specified group. If the user
        /// is not authorized (not a member or not assigned a role) returns 0.
        /// </returns>
        public static async Task<int> UserGroupAuthCheck(HearYeContext context, int userId, int messageGroupId)
        {
            if (userId < 1 || messageGroupId < 1)
            {
                return 0;
            }

            MessageGroupMember? mgm = await context.MessageGroupMembers!
                .Where(mgm => mgm.UserId == userId && mgm.MessageGroupId == messageGroupId)
                .FirstOrDefaultAsync();

            return mgm != null ? mgm.MessageGroupRoleId ?? 0 : 0;
        }

        /// <summary>
        /// Checks that the user is involved with the invite being requested.
        /// </summary>
        /// <param name="context">HearYeDatabaseContext.</param>
        /// <param name="userId">User id of user being checked.</param>
        /// <param name="inviteId">Invite.Id of specified invite.</param>
        /// <returns>
        /// 0 if the user is not involved in the invite.
        /// 1 if the user is the inviting user of the specified invite.
        /// 2 if the user is the invited user of the specified invite.
        /// </returns>
        public static async Task<int> UserInviteAuthCheck(HearYeContext context, int userId, int inviteId)
        {
            if (userId == 0 || inviteId == 0)
            {
                return 0;
            }

            MessageGroupInvitation? mgi = await context.MessageGroupInvitations!
                .Where(inv => inv.Id == inviteId
                    && (inv.InvitedUserId == userId || inv.InvitingUserId == userId))
                .FirstOrDefaultAsync();

            if (mgi is not null && mgi.InvitingUserId == userId)
            {
                return 1;
            }

            if (mgi is not null && mgi.InvitedUserId == userId)
            {
                return 2;
            }

            return 0;
        }
    }
}
