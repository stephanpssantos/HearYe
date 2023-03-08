using HearYe.Shared;

namespace HearYe.Client.Data
{
    public class StateContainer
    {
        private bool initiated = false;
        private string? userDbId;
        private IEnumerable<MessageGroup>? userGroups;
        private IEnumerable<MessageGroup>? userShortcutGroups;
        private int defaultGroupId;
        private int activeGroupId;
        private string? activeGroupName;
        private IEnumerable<PostWithUserName>? postCollection;
        private PostTypes activePostType = PostTypes.New;
        private ActiveLocations activeLocation = ActiveLocations.Index;
        private int postCount = 15;
        private int postSkip = 0;
        private bool modalIsOpen = false;
        private int acknowledgedModalId;
        private int membersModalId;
        private int inviteModalId;
        private int leaveModalId;

        public bool Initiated
        {
            get => initiated;
            set
            {
                if (initiated != value)
                {
                    initiated = value;
                    NotifyStateChanged();
                }
            }
        }

        public string? UserDbId { 
            get => userDbId; 
            set
            {
                if (userDbId != value)
                {
                    userDbId = value;
                    NotifyStateChanged();
                }
            }
        }

        public IEnumerable<MessageGroup>? UserGroups 
        { 
            get => userGroups;
            set 
            {
                if (userGroups != value)
                {
                    userGroups = value;
                    NotifyStateChanged();
                }
            } 
        }

        public IEnumerable<MessageGroup>? UserShortcutGroups
        {
            get => userShortcutGroups;
            set
            {
                if (userShortcutGroups != value)
                {
                    userShortcutGroups = value;
                    NotifyStateChanged();
                }
            }
        }

        public int DefaultGroupId
        {
            get => defaultGroupId;
            set
            {
                if (defaultGroupId != value)
                {
                    defaultGroupId = value;
                    NotifyStateChanged();
                }
            }
        }

        public int ActiveGroupId
        {
            get => activeGroupId;
            set
            {
                if (activeGroupId != value)
                {
                    activeGroupId = value;
                    NotifyStateChanged();
                }
            }
        }

        public string? ActiveGroupName
        {
            get => activeGroupName;
            set
            {
                if (activeGroupName != value)
                {
                    activeGroupName = value;
                    NotifyStateChanged();
                }
            }
        }

        public IEnumerable<PostWithUserName>? PostCollection
        {
            get => postCollection;
            set
            {
                if (postCollection != value)
                {
                    postCollection = value;
                    NotifyStateChanged();
                }
            }
        }

        public PostTypes ActivePostType
        {
            get => activePostType;
            set
            {
                if (activePostType != value)
                {
                    activePostType = value;
                    NotifyStateChanged();
                }
            }
        }

        public ActiveLocations ActiveLocation
        {
            get => activeLocation;
            set
            {
                if (activeLocation != value)
                {
                    activeLocation = value;
                    NotifyStateChanged();
                }
            }
        }

        public int PostCount
        {
            get => postCount;
            set
            {
                if (postCount != value)
                {
                    postCount = value;
                    NotifyStateChanged();
                }
            }
        }

        public int PostSkip
        {
            get => postSkip;
            set
            {
                if (postSkip != value)
                {
                    postSkip = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool ModalIsOpen
        {
            get => modalIsOpen;
            set
            {
                if (modalIsOpen!= value)
                {
                    modalIsOpen = value;
                    NotifyStateChanged();
                }
            }
        }

        public int AcknowledgedModalId
        {
            get => acknowledgedModalId;
            set
            {
                if (acknowledgedModalId != value)
                {
                    acknowledgedModalId = value;
                    NotifyStateChanged();
                }
            }
        }

        public int MembersModalId
        {
            get => membersModalId;
            set
            {
                if (membersModalId != value)
                {
                    membersModalId = value;
                    NotifyStateChanged();
                }
            }
        }

        public int InviteModalId
        {
            get => inviteModalId;
            set
            {
                if (inviteModalId != value)
                {
                    inviteModalId = value;
                    NotifyStateChanged();
                }
            }
        }

        public int LeaveModalId
        {
            get => leaveModalId;
            set
            {
                if (leaveModalId != value)
                {
                    leaveModalId = value;
                    NotifyStateChanged();
                }
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public enum PostTypes { New, Acknowledged, Stale }

    public enum ActiveLocations { Index, Groups, Invite, CreateGroup }
}
