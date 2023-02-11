﻿using HearYe.Shared;

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
                initiated = value;
                NotifyStateChanged();
            }
        }

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

        public IEnumerable<MessageGroup>? UserShortcutGroups
        {
            get => userShortcutGroups;
            set
            {
                userShortcutGroups = value;
                NotifyStateChanged();
            }
        }

        public int DefaultGroupId
        {
            get => defaultGroupId;
            set
            {
                defaultGroupId = value;
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

        public ActiveLocations ActiveLocation
        {
            get => activeLocation;
            set
            {
                activeLocation = value;
                NotifyStateChanged();
            }
        }

        public int PostCount
        {
            get => postCount;
            set
            {
                postCount = value;
                NotifyStateChanged();
            }
        }

        public int PostSkip
        {
            get => postSkip;
            set
            {
                postSkip = value;
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

    public enum ActiveLocations { Index, Groups, Invite, CreateGroup }
}
