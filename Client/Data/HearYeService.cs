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

        public Task<List<User>> GetUsersAsync()
        {
            return http.GetFromJsonAsync<List<User>>("api/users");
        }
    }
}
