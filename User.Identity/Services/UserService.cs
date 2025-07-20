using System.Net.Http;
using System;
using System.Threading.Tasks;


namespace User.Identity.Services;

public class UserService : IUserService
{
    private HttpClient _httpClient;
    private readonly string _userServiceUrl = "https://localhost:5201";

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_userServiceUrl);
    }

    public async Task<int> CheckOrCreate(string phone)
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("phone", phone)
        });
        var response = await _httpClient.PostAsync($"/api/users/check-or-create", form);


        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        else
        {
            return -1; // 或者抛出异常
        }
    }
}