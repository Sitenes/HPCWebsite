using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.DbContext;
using Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ViewModels;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class ApiResponseModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Created { get; set; }
    public string Updated { get; set; }
    public string Flavor { get; set; }
    public string AdminPass { get; set; }
    public string ProjectId { get; set; }
    public string UserId { get; set; }
}

public class OpenStackService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private string _accessToken;
    private DateTime _tokenExpiry;

    public OpenStackService(IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _accessToken = string.Empty;
        _tokenExpiry = DateTime.MinValue;
    }

    private async Task<string> GetValidTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        using var client = _httpClientFactory.CreateClient();
        var loginUrl = _configuration["Urls:OpenstackAutomator"] + $"/api/Authentication/login/{_configuration["OpenstackAutomatorApiUser:Username"]}";
        var loginData = _configuration["OpenstackAutomatorApiUser:Password"];
        var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(loginUrl, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = JsonConvert.DeserializeObject<ResultViewModel<TokenResponse>>(await response.Content.ReadAsStringAsync());
        _accessToken = tokenResponse.Data.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddMinutes(15);
        return _accessToken;
    }

    public async Task ProcessOrderAsync(HpcServerRentalOrder order)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DynamicDbContext>(); // Replace with your actual DbContext

            var client = _httpClientFactory.CreateClient();
            var token = await GetValidTokenAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _configuration["Urls:OpenstackAutomator"] + "/api/command/CreateCPUVM?";
            apiUrl += "flavor=" + order.Server.OpenstackFlavorName;
            apiUrl += "&imageOS=" + _configuration["OpenstackAutomatorApiUser:DefaultOSImage"];
          
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var rawResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString)["response"];

            // Parse the table-like response
            var apiResponse = new ApiResponseModel();
            var lines = rawResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("|") && !line.Contains("+----"))
                {
                    var parts = line.Split('|', StringSplitOptions.TrimEntries);
                    if (parts.Length < 3) continue;
                    var field = parts[1].Trim();
                    var value = parts[2].Trim();

                    switch (field)
                    {
                        case "id": apiResponse.Id = value; break;
                        case "name": apiResponse.Name = value; break;
                        case "status": apiResponse.Status = value; break;
                        case "created": apiResponse.Created = value; break;
                        case "updated": apiResponse.Updated = value; break;
                        case "flavor": apiResponse.Flavor = value; break;
                        case "adminPass": apiResponse.AdminPass = value; break;
                        case "project_id": apiResponse.ProjectId = value; break;
                        case "user_id": apiResponse.UserId = value; break;
                    }
                }
            }

            // Save to database
            var entity = new HpcServerRentalOrder
            {
                Id = order.Id,
                UserId = order.UserId,
                StartDate = order.StartDate,
                EndDate = order.EndDate,
                Status = OrderStatus.Completed,
                ServerId = order.ServerId,
                ServerName = apiResponse.Name,
                ServerSpecs = apiResponse.Flavor,
                CreatedAt = DateTime.Parse(apiResponse.Created),
                UpdatedAt = DateTime.Parse(apiResponse.Updated),
                WorkflowUserId = int.TryParse(apiResponse.UserId, out var userId) ? userId : order.WorkflowUserId
            };

            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log exception (use your logging mechanism)
            Console.WriteLine($"Error processing order {order.Id}: {ex.Message}");
        }
    }
}
