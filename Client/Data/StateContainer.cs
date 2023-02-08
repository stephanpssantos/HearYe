using HearYe.Shared;

namespace HearYe.Client.Data
{
    public class StateContainer
    {
        private string? userDbId;
        private IEnumerable<MessageGroup>? userGroups;
        private int activeGroupId;
        private string? activeGroupName;
        private IEnumerable<PostWithUserName>? postCollection;
        private PostTypes activePostType = PostTypes.New;
        private bool modalIsOpen = false;
        private int acknowledgedModalId;
        private int membersModalId;
        private int inviteModalId;
        private int leaveModalId;

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

        public IEnumerable<PostWithUserName>? PostCollection
        {
            get => postCollection;
            set
            {
                postCollection = value;
                NotifyStateChanged();
            }
        }

        public PostTypes ActivePostType
        {
            get => activePostType;
            set
            {
                activePostType = value;
                NotifyStateChanged();
            }
        }

        public bool ModalIsOpen
        {
            get => modalIsOpen;
            set
            {
                modalIsOpen = value;
                NotifyStateChanged();
            }
        }

        public int AcknowledgedModalId
        {
            get => acknowledgedModalId;
            set
            {
                acknowledgedModalId = value;
                NotifyStateChanged();
            }
        }

        public int MembersModalId
        {
            get => membersModalId;
            set
            {
                membersModalId = value;
                NotifyStateChanged();
            }
        }

        public int InviteModalId
        {
            get => inviteModalId;
            set
            {
                inviteModalId = value;
                NotifyStateChanged();
            }
        }

        public int LeaveModalId
        {
            get => leaveModalId;
            set
            {
                leaveModalId = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public enum PostTypes { New, Acknowledged, Stale }
}
