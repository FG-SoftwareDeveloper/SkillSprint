using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillSprint.ClientUI.Models;

namespace SkillSprint.ClientUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient http, ILogger<AuthService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { Email = email, Password = password };
                var resp = await _http.PostAsJsonAsync("api/Auth/login", payload);

                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
                    if (data == null)
                        return ServiceResult<LoginResponse>.Fail("Empty response from server.");
                    return ServiceResult<LoginResponse>.Success(data);
                }

                // Try parse ProblemDetails first
                try
                {
                    var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
                    var msg = problem?.Detail ?? problem?.Title ?? resp.ReasonPhrase ?? "Login failed.";
                    return ServiceResult<LoginResponse>.Fail(msg, problem);
                }
                catch
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    var msg = !string.IsNullOrWhiteSpace(text) ? text : resp.ReasonPhrase ?? "Login failed.";
                    return ServiceResult<LoginResponse>.Fail(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login request failed");
                return ServiceResult<LoginResponse>.Fail("Could not reach the server. Please try again.");
            }
        }
    }
}
