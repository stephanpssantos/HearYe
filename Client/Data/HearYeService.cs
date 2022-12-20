using HearYe.Shared;
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
            return http.GetFromJsonAsync<User?>($"api/users/{userId}");
        }

        public Task<User?> GetUserByOID(string userOidGuid)
        {
            return http.GetFromJsonAsync<User?>($"api/users/?aadOid={userOidGuid}");
        }

        public Task<List<MessageGroup>?> GetUserMessageGroupsAsync(string userId) 
        {
            return http.GetFromJsonAsync<List<MessageGroup>?>($"api/users/groups/{userId}");
        }
    }
}
