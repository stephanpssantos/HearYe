using Microsoft.AspNetCore.Components.Forms;
using HearYe.Shared;

namespace HearYe.Client.Shared
{
    public partial class InviteModal
    {
        private MessageGroupInvitation newInvite = new();
        private UserPublicInfo? invitee;
        private EditContext? editContext;
        private string? groupName;
        private string? bannerMessage;
        private string? bannerType;
        private bool displayConfirmation = false;
        private bool confirmationLock = false;

        protected override void OnParametersSet()
        {
            if (State.UserGroups is not null && State.InviteModalId != 0)
            {
                MessageGroup? group = State.UserGroups.Where(g => g.Id == State.InviteModalId).FirstOrDefault();
                if (group is not null)
                {
                    groupName = $"{group.MessageGroupName}";
                }
                else
                {
                    groupName = $"Unknown (ID: {State.InviteModalId})";
                }
            }
            else
            {
                groupName = "Unknown";
            }
        }

        protected override void OnInitialized()
        {
            editContext = new(newInvite);
            State.OnChange += StateHasChanged;
        }

        public void Dispose()
        {
            State.OnChange -= StateHasChanged;
        }

        private async void HandleSubmit()
        {
            bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
            if (!validDbId)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to send invite. Please try again later.";
                StateHasChanged();
                return;
            }

            newInvite.MessageGroupId = State.InviteModalId;
            newInvite.InvitingUserId = userDbId;
            newInvite.CreatedDate = DateTimeOffset.Now;
            if (editContext != null && editContext.Validate())
            {
                invitee = await Service.GetUserPublicInfoAsync(newInvite.InvitedUserId);
                if (invitee is null)
                {
                    bannerType = "alert-warning";
                    bannerMessage = "⚠ Specified user not found.";
                }
                else if (invitee.AcceptGroupInvitations == false)
                {
                    bannerType = "alert-info";
                    bannerMessage = "Specified user is not currently accepting invitations.";
                }
                else
                {
                    displayConfirmation = true;
                }

                StateHasChanged();
                return;
            }
        }

        private async void HandleConfirmation()
        {
            // Prevent multiple clicks while waiting.
            if (confirmationLock)
            {
                return;
            }

            confirmationLock = true;
            string? responseMessage = null;
            HttpResponseMessage response = await Service.NewMessageGroupInvitationAsync(newInvite);
            if (!response.IsSuccessStatusCode)
            {
                responseMessage = await response.Content.ReadAsStringAsync();
            }

            if (responseMessage is not null && responseMessage == "Duplicate invitation exists.")
            {
                bannerType = "alert-info";
                bannerMessage = "User has already been invited to this group.";
            }
            else if (responseMessage is not null && responseMessage == "Invited user is already a group member.")
            {
                bannerType = "alert-info";
                bannerMessage = "User is already a group member.";
            }
            else if (!response.IsSuccessStatusCode)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to send invite. Please try again later.";
            }
            else
            {
                bannerType = "alert-success";
                bannerMessage = "Invitation sent!";
            }

            newInvite = new();
            editContext = new(newInvite);
            confirmationLock = false;
            invitee = null;
            displayConfirmation = false;
            StateHasChanged();
        }

        private void CancelConfirmation()
        {
            bannerType = null;
            bannerMessage = null;
            invitee = null;
            displayConfirmation = false;
            StateHasChanged();
        }

        private void CloseModal()
        {
            State.MembersModalId = 0;
            State.ModalIsOpen = false;
        }
    }
}