using HearYe.Client.Data;
using HearYe.Shared;

namespace HearYe.Client.Pages
{
    public partial class ManageGroupInvitations
    {
        private bool acceptInvitations = true;
        private bool invitationsSentVisible = true;
        private bool invitationsReceivedVisible = true;
        private string? bannerMessage;
        private string? bannerType;
        private IEnumerable<MessageGroupInvitationWithNames>? invitationsSentList;
        private IEnumerable<MessageGroupInvitationWithNames>? invitationsReceivedList;
        private User? userInfo;

        protected override async Task OnParametersSetAsync()
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                Navigation.NavigateTo("/");
            }

            string? claimDbId = user.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
            bool validDbId = int.TryParse(claimDbId, out int userDbId);
            if (!validDbId)
            {
                Navigation.NavigateTo("/");
                return;
            }

            userInfo = await Service.GetUserAsync(claimDbId!);
            if (userInfo is null)
            {
                Navigation.NavigateTo("/");
                return;
            }

            acceptInvitations = userInfo.AcceptGroupInvitations;
            var invitationList = await Service.GetMessageGroupInvitationsAsync(userDbId);
            if (invitationList is not null)
            {
                invitationsSentList = invitationList.Where(i => i.InvitingUserId == userDbId).OrderByDescending(x => x.CreatedDate).Take(15);
                invitationsReceivedList = invitationList.Where(i => i.InvitedUserId == userDbId).OrderByDescending(x => x.CreatedDate).Take(15);
            }

            State.ActiveLocation = ActiveLocations.Invite;
        }

        private async void ToggleAcceptInvitation()
        {
            if (userInfo is null)
            {
                return;
            }

            userInfo.AcceptGroupInvitations = !userInfo.AcceptGroupInvitations;
            acceptInvitations = userInfo.AcceptGroupInvitations;
            StateHasChanged();
            var updateSuccessful = await Service.UpdateUserAsync(userInfo.Id, userInfo);
            if (!updateSuccessful.IsSuccessStatusCode)
            {
                userInfo.AcceptGroupInvitations = !userInfo.AcceptGroupInvitations;
                acceptInvitations = userInfo.AcceptGroupInvitations;
                bannerMessage = "⚠ Accept invitation toggle failed. Please try again later.";
                bannerType = "alert-warning";
                StateHasChanged();
            }
        }

        private async void DeleteInvitation(int invitationId)
        {
            var response = await Service.DeleteMessageGroupInvitationAsync(invitationId);
            if (response.IsSuccessStatusCode)
            {
                invitationsSentList = invitationsSentList!.Where(i => i.Id != invitationId).OrderByDescending(x => x.CreatedDate).ToList();
                StateHasChanged();
            }
            else
            {
                bannerMessage = "⚠ Invite deletion failed. Please try again later.";
                bannerType = "alert-warning";
                StateHasChanged();
            }
        }

        private async void AcceptInvitation(int invitationId)
        {
            var response = await Service.AcceptMessageGroupInvitationAsync(invitationId);
            if (response.IsSuccessStatusCode)
            {
                var acceptedInviteInfo = invitationsReceivedList!.Where(i => i.Id == invitationId).First();
                var newGroupInfo = await Service.GetMessageGroupAsync(acceptedInviteInfo.MessageGroupId);
                if (newGroupInfo is not null && State.UserGroups is not null)
                {
                    State.UserGroups = State.UserGroups.Append(newGroupInfo);
                }

                // Add new group to user's shortcut group list if that list has fewer than 5 entries.
                if (State.UserShortcutGroups is not null && State.UserShortcutGroups.Count() < 5)
                {
                    MessageGroupShortcut newShortcut = new()
                    {MessageGroupId = acceptedInviteInfo.MessageGroupId, UserId = acceptedInviteInfo.InvitedUserId};
                    var addToShortcuts = await Service.NewMessageGroupShortcutAsync(newShortcut);
                    if (addToShortcuts.IsSuccessStatusCode && newGroupInfo is not null)
                    {
                        // Add group to State.UserShortcutGroups.
                        var newShortcutGroupList = State.UserShortcutGroups.Append(newGroupInfo);
                        State.UserShortcutGroups = newShortcutGroupList;
                        // If user has not selected a default group, set this group as default.
                        if (userInfo is not null && (userInfo.DefaultGroupId is null || userInfo.DefaultGroupId == 0))
                        {
                            userInfo.DefaultGroupId = acceptedInviteInfo.MessageGroupId;
                            var updateUserDefaultGroup = await Service.UpdateUserAsync(userInfo.Id, userInfo);
                            if (updateUserDefaultGroup.IsSuccessStatusCode)
                            {
                                State.DefaultGroupId = acceptedInviteInfo.MessageGroupId;
                            }
                        }
                    }
                }

                invitationsReceivedList = invitationsReceivedList!.Where(i => i.Id != invitationId).OrderByDescending(x => x.CreatedDate).ToList();
                StateHasChanged();
            }
            else
            {
                bannerMessage = "⚠ Accept invite failed. Please try again later.";
                bannerType = "alert-warning";
                StateHasChanged();
            }
        }

        private async void DeclineInvitation(int invitationId)
        {
            var response = await Service.DeclineMessageGroupInvitationAsync(invitationId);
            if (response.IsSuccessStatusCode)
            {
                invitationsReceivedList = invitationsReceivedList!.Where(i => i.Id != invitationId).OrderByDescending(x => x.CreatedDate).ToList();
                StateHasChanged();
            }
            else
            {
                bannerMessage = "⚠ Decline invite failed. Please try again later.";
                bannerType = "alert-warning";
                StateHasChanged();
            }
        }

        private void ToggleInvitationsSentVisible()
        {
            invitationsSentVisible = !invitationsSentVisible;
        }

        private void ToggleInvitationsReceivedVisible()
        {
            invitationsReceivedVisible = !invitationsReceivedVisible;
        }
    }
}