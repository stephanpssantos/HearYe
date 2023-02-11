﻿using HearYe.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net;
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

        public Task<User?> GetUserAsync(string userId)
        {
            return http.GetFromJsonAsync<User?>($"api/user/{userId}");
        }

        public Task<User?> GetUserByOIDAsync(string userOidGuid)
        {
            return http.GetFromJsonAsync<User?>($"api/user/?aadOid={userOidGuid}");
        }

        public Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId) 
        {
            return http.GetFromJsonAsync<List<MessageGroup>?>($"api/user/groups/{userId}");
        }

        public async Task<User?> NewUserAsync(User user)
        {
            try
            {
                HttpResponseMessage response = await http.PostAsJsonAsync("api/user", user);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<User>();
                }
                else
                {
                    return null;
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                //Navigation.NavigateToLogin("authentication/login"); in case ex.redirect causes issues
                ex.Redirect();
                return null;
            }
            catch (Exception ex)
            {
                // Do something here
                return null;
            }
        }

        public async Task<Post?> NewPostAsync(Post post)
        {
            try
            {
                HttpResponseMessage response = await http.PostAsJsonAsync("api/post", post);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Post>();
                }
                else
                {
                    return null;
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                //Navigation.NavigateToLogin("authentication/login"); in case ex.redirect causes issues
                ex.Redirect();
                return null;
            }
            catch (Exception ex)
            {
                // Do something here
                return null;
            }
        }

        public Task<List<PostWithUserName>?> GetNewPostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return http.GetFromJsonAsync<List<PostWithUserName>?>($"api/post/new?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public Task<List<PostWithUserName>?> GetAcknowledgedPostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return http.GetFromJsonAsync<List<PostWithUserName>?>($"api/post/acknowledged?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public Task<List<PostWithUserName>?> GetStalePostsAsync(string messageGroupId, int count = 15, int skip = 0)
        {
            // Get 1 more than usual so we know whether or not to active the 'next' button.
            count = ++count;
            return http.GetFromJsonAsync<List<PostWithUserName>?>($"api/post/stale?messageGroupId={messageGroupId}&count={count}&skip={skip}");
        }

        public async Task<HttpResponseMessage> NewAcknowledgementAsync(Acknowledgement acknowledgement)
        {
            return await http.PostAsJsonAsync("api/acknowledgement", acknowledgement);
        }

        public async Task<HttpResponseMessage> DeleteAcknowledgementAsync(int postId, int userId)
        {
            return await http.DeleteAsync($"api/acknowledgement/delete?postId={postId}&userId={userId}");
        }

        public Task<List<UserPublicInfo>?> GetPostAcknowledgedUsersAsync(int userId)
        {
            return http.GetFromJsonAsync<List<UserPublicInfo>?>($"api/post/acknowledged/{userId}");
        }

        public async Task<MessageGroup?> NewMessageGroupAsync(string newGroupName)
        {
            try
            {
                HttpResponseMessage response = await http.PostAsJsonAsync("api/messagegroup/new", newGroupName);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MessageGroup>();
                }
                else
                {
                    return null;
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                //Navigation.NavigateToLogin("authentication/login"); in case ex.redirect causes issues
                ex.Redirect();
                return null;
            }
            catch (Exception ex)
            {
                // Do something here
                return null;
            }
        }

        public Task<List<MessageGroupInvitationWithNames>?> GetMessageGroupInvitationsAsync(int userId)
        {
            return http.GetFromJsonAsync<List<MessageGroupInvitationWithNames>?>($"api/messagegroupinvitation/user/{userId}");
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupInvitationAsync(int inviteId)
        {
            return await http.DeleteAsync($"api/messagegroupinvitation/delete/{inviteId}");
        }

        public async Task<HttpResponseMessage> UpdateUserAsync(int id, User user)
        {
            return await http.PutAsJsonAsync($"api/user/{id}", user);
        }

        public async Task<HttpResponseMessage> AcceptMessageGroupInvitationAsync(int id)
        {
            return await http.PatchAsync($"api/messagegroupinvitation/accept/{id}", null);
        }

        public async Task<HttpResponseMessage> DeclineMessageGroupInvitationAsync(int id)
        {
            return await http.PatchAsync($"api/messagegroupinvitation/decline/{id}", null);
        }

        public async Task<List<MessageGroupMemberWithName>?> GetMessageGroupMembersAsync(int id)
        {
            return await http.GetFromJsonAsync<List<MessageGroupMemberWithName>?>($"api/messagegroup/members/{id}");
        }

        public async Task<MessageGroup?> GetMessageGroupAsync(int id)
        {
            return await http.GetFromJsonAsync<MessageGroup?>($"api/messagegroup/{id}");
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupMemberAsync(int userId, int groupId)
        {
            return await http.DeleteAsync($"api/messagegroup/member?userId={userId}&groupId={groupId}");
        }

        public async Task<HttpResponseMessage> SetMessageGroupRoleAsync(MessageGroupMember mgm)
        {
            return await http.PutAsJsonAsync("api/messagegroup/setrole", mgm);
        }

        public async Task<UserPublicInfo?> GetUserPublicInfoAsync(int userId)
        {
            HttpResponseMessage response = await http.GetAsync($"api/user/public/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserPublicInfo>();
            }
            else
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> NewMessageGroupInvitationAsync(MessageGroupInvitation inv)
        {
            // This could immediately be transformed to a MessageGroupInvitation but
            // the HttpResponseMessage is used to determine response status.
            return await http.PostAsJsonAsync("api/messagegroupinvitation/new", inv);
        }

        public async Task<List<MessageGroup>?> GetMessageGroupShortcutsAsync(int id)
        {
            return await http.GetFromJsonAsync<List<MessageGroup>?>($"api/messagegroupshortcut/{id}");
        }

        public async Task<HttpResponseMessage> NewMessageGroupShortcutAsync(MessageGroupShortcut shortcut)
        {
            return await http.PostAsJsonAsync("api/messagegroupshortcut/new", shortcut);
        }

        public async Task<HttpResponseMessage> DeleteMessageGroupShortcutAsync(int userId, int groupId)
        {
            return await http.DeleteAsync($"api/messagegroupshortcut/delete?userId={userId}&groupId={groupId}");
        }
    }
}
