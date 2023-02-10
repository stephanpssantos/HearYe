using HearYe.Shared;

namespace HearYe.Client.Data
{
    public interface IHearYeService
    {
        Task<User?> GetUserAsync(string userId);
        Task<User?> GetUserByOIDAsync(string userOidGuid);
        Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId);
        Task<User?> NewUserAsync(User user);
        Task<List<PostWithUserName>?> GetNewPostsAsync(string messageGroupId, int count = 15, int skip = 0);
        Task<List<PostWithUserName>?> GetAcknowledgedPostsAsync(string messageGroupId, int count = 15, int skip = 0);
        Task<List<PostWithUserName>?> GetStalePostsAsync(string messageGroupId, int count = 15, int skip = 0);
        Task<Post?> NewPostAsync(Post post);
        Task<HttpResponseMessage> NewAcknowledgementAsync(Acknowledgement acknowledgement);
        Task<HttpResponseMessage> DeleteAcknowledgementAsync(int postId, int userId);
        Task<List<UserPublicInfo>?> GetPostAcknowledgedUsersAsync(int userId);
        Task<MessageGroup?> NewMessageGroupAsync(string newGroupName);
        Task<List<MessageGroupInvitationWithNames>?> GetMessageGroupInvitationsAsync(int userId);
        Task<HttpResponseMessage> DeleteMessageGroupInvitationAsync(int inviteId);
        Task<HttpResponseMessage> UpdateUserAsync(int id, User user);
        Task<HttpResponseMessage> AcceptMessageGroupInvitationAsync(int id);
        Task<HttpResponseMessage> DeclineMessageGroupInvitationAsync(int id);
        Task<List<MessageGroupMemberWithName>?> GetMessageGroupMembersAsync(int id);
        Task<MessageGroup?> GetMessageGroupAsync(int id);
        Task<HttpResponseMessage> DeleteMessageGroupMemberAsync(int userId, int groupId);
        Task<HttpResponseMessage> SetMessageGroupRoleAsync(MessageGroupMember mgm);
        Task<UserPublicInfo?> GetUserPublicInfoAsync(int userId);
        Task<HttpResponseMessage> NewMessageGroupInvitationAsync(MessageGroupInvitation inv);
        Task<List<MessageGroup>?> GetMessageGroupShortcutsAsync(int id);
        Task<HttpResponseMessage> NewMessageGroupShortcutAsync(MessageGroupShortcut shortcut);
        Task<HttpResponseMessage> DeleteMessageGroupShortcutAsync(int userId, int groupId);
    }
}
