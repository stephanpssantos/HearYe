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
    }
}
