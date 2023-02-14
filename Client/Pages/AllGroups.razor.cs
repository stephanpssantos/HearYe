using HearYe.Client.Data;
using HearYe.Shared;

namespace HearYe.Client.Pages
{
    public partial class AllGroups
    {
        private string? bannerMessage;
        private string? bannerType;
        private MessageGroup? defaultMessageGroup;

        private async void AddToShortcuts(int groupId)
        {
            bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
            if (!validDbId)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to add shortcut. Please try again later.";
                StateHasChanged();
                return;
            }

            MessageGroupShortcut newShortcut = new()
            {UserId = userDbId, MessageGroupId = groupId, };
            var added = await Service.NewMessageGroupShortcutAsync(newShortcut);
            if (added.IsSuccessStatusCode)
            {
                var groupToAdd = State.UserGroups!.First(g => g.Id == groupId);
                State.UserShortcutGroups = State.UserShortcutGroups!.Append(groupToAdd);
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to add shortcut. Please try again later.";
                StateHasChanged();
            }
        }

        private async void RemoveFromShortcuts(int groupId)
        {
            bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
            if (!validDbId)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to remove shortcut. Please try again later.";
                StateHasChanged();
                return;
            }

            var removed = await Service.DeleteMessageGroupShortcutAsync(userDbId, groupId);
            if (removed.IsSuccessStatusCode)
            {
                State.UserShortcutGroups = State.UserShortcutGroups!.Where(g => g.Id != groupId).ToList();
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to remove shortcut. Please try again later.";
                StateHasChanged();
            }
        }

        private async void SetDefaultGroup(MessageGroup group)
        {
            if (State.DefaultGroupId == group.Id)
            {
                return;
            }

            var getUser = await Service.GetUserAsync(State.UserDbId ?? "0");
            if (getUser is null)
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to set default group. Please try again later.";
                StateHasChanged();
                return;
            }

            getUser.DefaultGroupId = group.Id;
            var updateUser = await Service.UpdateUserAsync(getUser.Id, getUser);
            if (updateUser.IsSuccessStatusCode)
            {
                defaultMessageGroup = group;
                State.DefaultGroupId = group.Id;
            }
            else
            {
                bannerType = "alert-warning";
                bannerMessage = "⚠ Failed to set default group. Please try again later.";
                StateHasChanged();
            }

            return;
        }

        private void CheckLoadGroups()
        {
            if (State.Initiated && State.UserGroups is null)
            {
                LoadGroups();
            }
            else if (State.Initiated && State.UserGroups is not null && defaultMessageGroup is null)
            {
                defaultMessageGroup = State.UserGroups.FirstOrDefault(g => g.Id == State.DefaultGroupId);
            }
        }

        private async void LoadGroups()
        {
            var userGroups = await Service.GetUserMessageGroupsAsync(State.UserDbId!);
            if (userGroups is not null)
            {
                defaultMessageGroup = userGroups.FirstOrDefault(g => g.Id == State.DefaultGroupId);
            }

            State.UserGroups = userGroups;
        }

        protected override void OnInitialized()
        {
            State.OnChange += StateHasChanged;
            State.OnChange += CheckLoadGroups;
            State.ActiveLocation = ActiveLocations.Groups;
        }

        public void Dispose()
        {
            State.OnChange -= StateHasChanged;
            State.OnChange -= CheckLoadGroups;
        }

        private async void GoToGroup(int groupId)
        {
            var groupInfo = State.UserGroups!.First(g => g.Id == groupId);
            var groupPosts = await Service.GetNewPostsAsync(groupId.ToString(), State.PostCount, State.PostSkip);
            State.PostCollection = groupPosts;
            State.ActiveGroupId = groupId;
            State.ActiveGroupName = groupInfo.MessageGroupName;
            Navigation.NavigateTo("/");
        }

        private void OpenModal(int id, int modal)
        {
            State.MembersModalId = modal == 1 ? id : 0;
            State.InviteModalId = modal == 2 ? id : 0;
            State.LeaveModalId = modal == 3 ? id : 0;
            State.ModalIsOpen = true;
        }
    }
}