using HearYe.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace HearYe.Client.Data
{
    internal static class HttpClientExtensions
    {
        public static async Task<TValue?> TryGetFromJsonAsync<TValue>(this HttpClient client, string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TValue>();
                }
                else
                {
                    return default(TValue);
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                //Navigation.NavigateToLogin("authentication/login"); in case ex.redirect causes issues
                ex.Redirect();
                return default(TValue);
            }
            catch (Exception ex)
            {
                //If using a monitoring service, place a call to it here.
                Console.WriteLine(ex.Message);
                return default(TValue);
            }
        }

        public static async Task<TValue?> TryPostAsJsonAsync<TValue>(this HttpClient client, string url, object payload)
        {
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url, payload);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TValue>();
                }
                else
                {
                    return default(TValue);
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
                return default(TValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(TValue);
            }
        }
    }
}
