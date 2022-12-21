using HearYe.Shared;

namespace HearYe.Client.Data
{
    public interface IHearYeService
    {
        Task<User?> GetUserAsync(string userId);
        Task<User?> GetUserByOIDAsync(string userOidGuid);
        Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId);
        Task<User?> NewUserAsync(User user);
    }
}
