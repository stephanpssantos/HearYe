using HearYe.Shared;

namespace HearYe.Client.Data
{
    public interface IHearYeService
    {
        Task<User?> GetUserAsync(string userId); //user
        Task<User?> GetUserByOIDAsync(string userOidGuid);//user
        Task<UserPublicInfo?> GetUserPublicInfoAsync(int userId); //user
        Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId);//user
        Task<User?> NewUserAsync(User user);//user
        Task<HttpResponseMessage> UpdateUserAsync(int id, User user); //user
        Task<List<PostWithUserName>?> GetNewPostsAsync(string messageGroupId, int count = 15, int skip = 0);//post
        Task<List<PostWithUserName>?> GetAcknowledgedPostsAsync(string messageGroupId, int count = 15, int skip = 0);//post
        Task<List<PostWithUserName>?> GetStalePostsAsync(string messageGroupId, int count = 15, int skip = 0);//post
        Task<List<UserPublicInfo>?> GetPostAcknowledgedUsersAsync(int userId); //post
        Task<Post?> NewPostAsync(Post post);//post
        Task<MessageGroup?> GetMessageGroupAsync(int id); //messagegroup
        Task<List<MessageGroupMemberWithName>?> GetMessageGroupMembersAsync(int id); //messagegroup
        Task<MessageGroup?> NewMessageGroupAsync(string newGroupName); //messagegroup
        Task<HttpResponseMessage> SetMessageGroupRoleAsync(MessageGroupMember mgm); //messagegroup
        Task<HttpResponseMessage> DeleteMessageGroupMemberAsync(int userId, int groupId); //messagegroup
        Task<List<MessageGroupInvitationWithNames>?> GetMessageGroupInvitationsAsync(int userId); //mgi
        Task<HttpResponseMessage> NewMessageGroupInvitationAsync(MessageGroupInvitation inv); //mgi
        Task<HttpResponseMessage> AcceptMessageGroupInvitationAsync(int id); //mgi
        Task<HttpResponseMessage> DeclineMessageGroupInvitationAsync(int id); //mgi
        Task<HttpResponseMessage> DeleteMessageGroupInvitationAsync(int inviteId); //mgi
        Task<HttpResponseMessage> NewAcknowledgementAsync(Acknowledgement acknowledgement); //ack
        Task<HttpResponseMessage> DeleteAcknowledgementAsync(int postId, int userId); //ack
        Task<List<MessageGroup>?> GetMessageGroupShortcutsAsync(int id); //mgs
        Task<HttpResponseMessage> NewMessageGroupShortcutAsync(MessageGroupShortcut shortcut); //mgs
        Task<HttpResponseMessage> DeleteMessageGroupShortcutAsync(int userId, int groupId);//mgs
    }
}
