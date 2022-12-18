using HearYe.Shared;

namespace HearYe.Client.Data
{
    public interface IHearYeService
    {
        Task<List<User>?> GetUsersAsync();
    }
}
