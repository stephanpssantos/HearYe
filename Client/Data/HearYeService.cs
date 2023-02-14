using HearYe.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json; // GetFromJsonAsync, ReadFromJsonAsync

namespace HearYe.Client.Data
{
    public class HearYeService : IHearYeService
    {
        private readonly HttpClient http;

        public HearYeService(HttpClient http)
        {
            this.http = http;
        }

        #region USER
        public async Task<User?> GetUserAsync(string userId)
        {
            return await http.TryGetFromJsonAsync<User?>($"api/user/{userId}");
        }

        public async Task<User?> GetUserByOIDAsync(string userOidGuid)
        {
            return await http.TryGetFromJsonAsync<User?>($"api/user/?aadOid={userOidGuid}");
        }

        public async Task<UserPublicInfo?> GetUserPublicInfoAsync(int userId)
        {
            return await http.TryGetFromJsonAsync<UserPublicInfo?>($"api/user/public/{userId}");
        }

        public async Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId) 
        {
            return await http.TryGetFromJsonAsync<List<MessageGroup>?>($"api/user/groups/{userId}");
        }

        public async Task<User?> NewUserAsync(User user)
        {
            return await http.TryPostAsJsonAsync<User?>("api/user", user);
        }

        public async Task<HttpResponseMessage> UpdateUserAsync(int id, User user)
        {
            return await http.PutAsJsonAsync($"api/user/{id}", user);
        }
        #endregion USER

        #region POST
        public async Task<List<PostWithUserName>?> GetNewPostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return await http.TryGetFromJsonAsync<List<PostWithUserName>?>($"api/post/new?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public async Task<List<PostWithUserName>?> GetAcknowledgedPostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return await http.TryGetFromJsonAsync<List<PostWithUserName>?>($"api/post/acknowledged?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public async Task<List<PostWithUserName>?> GetStalePostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return await http.TryGetFromJsonAsync<List<PostWithUserName>?>($"api/post/stale?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public async Task<List<UserPublicInfo>?> GetPostAcknowledgedUsersAsync(int userId)
        {
            return await http.TryGetFromJsonAsync<List<UserPublicInfo>?>($"api/post/acknowledged/{userId}");
        }

        public async Task<Post?> NewPostAsync(Post post)
        {
            return await http.TryPostAsJsonAsync<Post?>("api/post", post);
        }
        #endregion POST

        #region MESSAGE GROUP
        public async Task<MessageGroup?> GetMessageGroupAsync(int id)
        {
            return await http.TryGetFromJsonAsync<MessageGroup?>($"api/messagegroup/{id}");
        }

        public async Task<List<MessageGroupMemberWithName>?> GetMessageGroupMembersAsync(int id)
        {
            return await http.TryGetFromJsonAsync<List<MessageGroupMemberWithName>?>($"api/messagegroup/members/{id}");
        }

        public async Task<MessageGroup?> NewMessageGroupAsync(string newGroupName)
        {
            return await http.TryPostAsJsonAsync<MessageGroup?>("api/messagegroup/new", newGroupName);
        }

        public async Task<HttpResponseMessage> SetMessageGroupRoleAsync(MessageGroupMember mgm)
        {
            return await http.PutAsJsonAsync("api/messagegroup/setrole", mgm);
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupMemberAsync(int userId, int groupId)
        {
            return await http.DeleteAsync($"api/messagegroup/member?userId={userId}&groupId={groupId}");
        }
        #endregion MESSAGE GROUP

        #region MESSAGE GROUP INVITATIONS
        public async Task<List<MessageGroupInvitationWithNames>?> GetMessageGroupInvitationsAsync(int userId)
        {
            return await http.TryGetFromJsonAsync<List<MessageGroupInvitationWithNames>?>($"api/messagegroupinvitation/user/{userId}");
        }

        public async Task<HttpResponseMessage> NewMessageGroupInvitationAsync(MessageGroupInvitation inv)
        {
            // This could immediately be transformed to a MessageGroupInvitation but
            // the HttpResponseMessage is used to determine response status.
            return await http.PostAsJsonAsync("api/messagegroupinvitation/new", inv);
        }

        public async Task<HttpResponseMessage> AcceptMessageGroupInvitationAsync(int id)
        {
            return await http.PatchAsync($"api/messagegroupinvitation/accept/{id}", null);
        }

        public async Task<HttpResponseMessage> DeclineMessageGroupInvitationAsync(int id)
        {
            return await http.PatchAsync($"api/messagegroupinvitation/decline/{id}", null);
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupInvitationAsync(int inviteId)
        {
            return await http.DeleteAsync($"api/messagegroupinvitation/delete/{inviteId}");
        }
        #endregion MESSAGE GROUP INVITATIONS

        #region ACKNOWLEDGEMENTS
        public async Task<HttpResponseMessage> NewAcknowledgementAsync(Acknowledgement acknowledgement)
        {
            return await http.PostAsJsonAsync("api/acknowledgement", acknowledgement);
        }

        public async Task<HttpResponseMessage> DeleteAcknowledgementAsync(int postId, int userId)
        {
            return await http.DeleteAsync($"api/acknowledgement/delete?postId={postId}&userId={userId}");
        }
        #endregion ACKNOWLEDGEMENTS

        #region MESSAGE GROUP SHORTCUTS
        public async Task<List<MessageGroup>?> GetMessageGroupShortcutsAsync(int id)
        {
            return await http.TryGetFromJsonAsync<List<MessageGroup>?>($"api/messagegroupshortcut/{id}");
        }

        public async Task<HttpResponseMessage> NewMessageGroupShortcutAsync(MessageGroupShortcut shortcut)
        {
            return await http.PostAsJsonAsync("api/messagegroupshortcut/new", shortcut);
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupShortcutAsync(int userId, int groupId)
        {
            return await http.DeleteAsync($"api/messagegroupshortcut/delete?userId={userId}&groupId={groupId}");
        }
        #endregion MESSAGE GROUP SHORTCUTS
    }
}
