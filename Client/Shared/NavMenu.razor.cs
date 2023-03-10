using HearYe.Shared;

namespace HearYe.Client.Shared
{
    public partial class NavMenu
    {
        protected override void OnInitialized()
        {
            State.OnChange += StateHasChanged;
        }

        public void Dispose()
        {
            State.OnChange -= StateHasChanged;
        }

        protected override void OnParametersSet()
        {
            if (State.UserDbId is null)
            {
                InitializeState();
            }
        }

        private async void InitializeState()
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                string? userDbId = user.Claims.FirstOrDefault(x => x.Type.Equals("extension_DatabaseId"))?.Value;
                // The code below checks that the newUser token claim exists - but it might cause problems.
                // if (user.Claims.FirstOrDefault(x => x.Type.Equals("newUser"))?.Value is not null && userDbId is null)
                if (userDbId is null)
                {
                    Navigation.NavigateTo("/newuser");
                    return;
                }

                State.UserDbId = userDbId;
                User? userDbInfo = await Service.GetUserAsync(userDbId);
                if (userDbInfo is null)
                {
                    Navigation.NavigateTo("/newuser");
                    return;
                }

                var userShortcutGroups = await Service.GetMessageGroupShortcutsAsync(userDbInfo.Id);
                State.DefaultGroupId = userDbInfo.DefaultGroupId is null ? 0 : (int)userDbInfo.DefaultGroupId;
                State.UserShortcutGroups = userShortcutGroups ?? new();
                MessageGroup? defaultGroup = null;
                MessageGroup? defaultGroupInShortcutGroups = State.UserShortcutGroups.FirstOrDefault(g => g.Id == State.DefaultGroupId);
                IEnumerable<PostWithUserName>? activeGroupPosts;
                if (State.DefaultGroupId != 0 && defaultGroupInShortcutGroups is not null)
                {
                    defaultGroup = defaultGroupInShortcutGroups;
                }
                else if (State.DefaultGroupId != 0)
                {
                    defaultGroup = await Service.GetMessageGroupAsync(State.DefaultGroupId);
                }

                // If no authorization to access group or group no longer exists or default not chosen.
                if (defaultGroup is null || State.DefaultGroupId == 0)
                {
                    if (userShortcutGroups is not null && userShortcutGroups.Any())
                    {
                        var groupId = userShortcutGroups.First().Id.ToString();
                        activeGroupPosts = await Service.GetNewPostsAsync(groupId, State.PostCount, State.PostSkip);
                        State.PostCollection = activeGroupPosts;
                        State.ActiveGroupId = userShortcutGroups.First().Id;
                        State.ActiveGroupName = userShortcutGroups.First().MessageGroupName;
                    }
                    else
                    {
                        State.PostCollection = new List<PostWithUserName>();
                    }
                // If no default group chosen and no groups favorited, load nothing.
                }
                else
                {
                    activeGroupPosts = await Service.GetNewPostsAsync(State.DefaultGroupId.ToString(), State.PostCount, State.PostSkip);
                    State.PostCollection = activeGroupPosts;
                    State.ActiveGroupId = defaultGroup.Id;
                    State.ActiveGroupName = defaultGroup.MessageGroupName;
                }

                State.Initiated = true;
            }
        }
    }
}