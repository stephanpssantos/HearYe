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
            HttpResponseMessage response = await http.PostAsJsonAsync("api/user", user);

            return await response.Content.ReadFromJsonAsync<User>();
        }
    }
}
