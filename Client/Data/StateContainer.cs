using HearYe.Shared;

namespace HearYe.Client.Data
{
    public class StateContainer
    {
        private string? userDbId;
        private IEnumerable<MessageGroup>? userGroups;
        private int activeGroupId;
        private string? activeGroupName;
        private IEnumerable<Post>? postCollection;

        public string? UserDbId { 
            get => userDbId; 
            set
            {
                userDbId = value;
                NotifyStateChanged();
            }
        }

        public IEnumerable<MessageGroup>? UserGroups 
        { 
            get => userGroups;
            set 
            {
                userGroups = value;
                NotifyStateChanged();
            } 
        }

        public int ActiveGroupId
        {
            get => activeGroupId;
            set
            {
                activeGroupId = value;
                NotifyStateChanged();
            }
        }

        public string? ActiveGroupName
        {
            get => activeGroupName;
            set
            {
                activeGroupName = value;
                NotifyStateChanged();
            }
        }

        public IEnumerable<Post>? PostCollection
        {
            get => postCollection;
            set
            {
                postCollection = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
