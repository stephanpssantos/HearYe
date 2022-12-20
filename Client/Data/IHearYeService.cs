using HearYe.Shared;

namespace HearYe.Client.Data
{
    public interface IHearYeService
    {
        Task<User?> GetUserAsync(string userId);
        Task<User?> GetUserByOID(string userOidGuid);
        Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId);
    }
}
