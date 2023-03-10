using HearYe.Shared;

namespace HearYe.Client.Shared
{
    public partial class MembersModal
    {
        private bool displayError = false;
        private bool displayConfirmation = false;
        private int userRole;
        private int userId;
        private int confirmationUserId;
        private string modalTitle = "Loading...";
        private string? confirmationUserName;
        private string? bannerMessage;
        private string? bannerType;
        private MessageGroupMemberWithName? confirmationUser;
        private IEnumerable<MessageGroupMemberWithName>? userList;

        protected override async Task OnParametersSetAsync()
        {
            if (State.ModalIsOpen && State.MembersModalId > 0)
            {
                userList = await Service.GetMessageGroupMembersAsync(State.MembersModalId);
                bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
                if (userList is null || !validDbId)
                {
                    displayError = true;
                    return;
                }

                var user = userList.Where(u => u.UserId == userDbId).FirstOrDefault();
                if (State.UserGroups is not null && user is not null && user.MessageGroupRoleId is not null)
                {
                    userRole = (int)user.MessageGroupRoleId;
                    userId = userDbId;
                    MessageGroup? group = State.UserGroups.Where(g => g.Id == State.MembersModalId).FirstOrDefault();
                    if (group is not null)
                    {
                        modalTitle = group.MessageGroupName + " members";
                    }
                    else
                    {
                        modalTitle = "Group members";
                    }
                }
                else
                {
                    displayError = true;
                    return;
                }

                StateHasChanged();
            }
        }

        protected override void OnInitialized()
        {
            State.OnChange += StateHasChanged;
        }

        public void Dispose()
        {
            State.OnChange -= StateHasChanged;
        }

        private async void MakeUserMember(MessageGroupMemberWithName mgmn)
        {
            MessageGroupMember mgm = new()
            {Id = mgmn.Id, MessageGroupId = mgmn.MessageGroupId, UserId = mgmn.UserId, MessageGroupRoleId = 2, };
            var response = await Service.SetMessageGroupRoleAsync(mgm);
            if (response.IsSuccessStatusCode)
            {
                var userToUpdate = userList!.FirstOrDefault(u => u.Id == mgmn.Id);
                if (userToUpdate is not null)
                {
                    userToUpdate.MessageGroupRoleId = 2;
                }
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ User role assignment failed. Please try again later.";
            }

            StateHasChanged();
        }

        private async void MakeUserAdmin(MessageGroupMemberWithName mgmn)
        {
            MessageGroupMember mgm = new()
            {Id = mgmn.Id, MessageGroupId = mgmn.MessageGroupId, UserId = mgmn.UserId, MessageGroupRoleId = 1, };
            var response = await Service.SetMessageGroupRoleAsync(mgm);
            if (response.IsSuccessStatusCode)
            {
                var userToUpdate = userList!.FirstOrDefault(u => u.Id == mgmn.Id);
                if (userToUpdate is not null)
                {
                    userToUpdate.MessageGroupRoleId = 1;
                }
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ User role assignment failed. Please try again later.";
            }

            StateHasChanged();
        }

        private void DisplayConfirmation(MessageGroupMemberWithName user)
        {
            displayConfirmation = true;
            confirmationUser = user;
            confirmationUserId = user.UserId;
            confirmationUserName = user.UserName;
        }

        private void CloseConfirmation()
        {
            displayConfirmation = false;
            confirmationUser = null;
            confirmationUserId = 0;
            confirmationUserName = String.Empty;
            StateHasChanged();
        }

        private async void ConfirmRemoval()
        {
            var response = await Service.DeleteMessageGroupMemberAsync(confirmationUser!.Id, State.MembersModalId);
            if (response.IsSuccessStatusCode)
            {
                userList = userList!.Where(u => u.UserId != confirmationUserId).ToList();
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ User removal failed. Please try again later.";
            }

            CloseConfirmation();
        }

        private void CloseModal()
        {
            State.MembersModalId = 0;
            State.ModalIsOpen = false;
        }
    }
}