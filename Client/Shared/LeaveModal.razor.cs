using HearYe.Shared;

namespace HearYe.Client.Shared
{
    public partial class LeaveModal
    {
        private string? groupName;
        private string? bannerMessage;
        private string? bannerType;
        private bool displayConfirmation = false;

        protected override void OnParametersSet()
        {
            if (State.UserGroups is not null && State.LeaveModalId != 0)
            {
                MessageGroup? group = State.UserGroups.Where(g => g.Id == State.LeaveModalId).FirstOrDefault();
                if (group is not null)
                {
                    groupName = $"{group.MessageGroupName}";
                }
                else
                {
                    groupName = $"Unknown (ID: {State.LeaveModalId})";
                }
            }
            else
            {
                groupName = "Unknown";
            }
        }

        private async void ConfirmLeave()
        {
            bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
            if (!validDbId)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to leave group. Please try again later.";
                return;
            }

            var response = await Service.DeleteMessageGroupMemberAsync(userDbId, State.LeaveModalId);
            if (!response.IsSuccessStatusCode)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to leave group. Please try again later.";
            }
            else
            {
                bannerType = null;
                bannerMessage = null;
                displayConfirmation = true;
                State.UserGroups = State.UserGroups!.Where(g => g.Id != State.LeaveModalId).ToList();
                // Check if group is the user's default group. Set user default group to null if it is.
                var userDbInfo = await Service.GetUserAsync(State.UserDbId!);
                if (userDbInfo is not null && userDbInfo.DefaultGroupId == State.LeaveModalId)
                {
                    userDbInfo.DefaultGroupId = null;
                    var updateUser = await Service.UpdateUserAsync(userDbInfo.Id, userDbInfo);
                    State.DefaultGroupId = 0;
                }

                // if current group is the active group, reset it.
                if (State.LeaveModalId == State.ActiveGroupId)
                {
                    State.ActiveGroupName = "No group selected";
                    State.ActiveGroupId = 0;
                    State.PostCollection = new List<PostWithUserName>();
                }

                // Check if group is in shortcuts. Remove it from shortcuts if it is.
                if (State.UserShortcutGroups is not null)
                {
                    var groupInShorcuts = State.UserShortcutGroups.FirstOrDefault(g => g.Id == State.LeaveModalId);
                    if (groupInShorcuts is not null)
                    {
                        var removeFromShortcuts = await Service.DeleteMessageGroupShortcutAsync(userDbId, State.LeaveModalId);
                    }

                    State.UserShortcutGroups = State.UserShortcutGroups.Where(g => g.Id != State.LeaveModalId).ToList();
                }
            }

            StateHasChanged();
        }

        private void CloseModal()
        {
            State.LeaveModalId = 0;
            State.ModalIsOpen = false;
        }
    }
}