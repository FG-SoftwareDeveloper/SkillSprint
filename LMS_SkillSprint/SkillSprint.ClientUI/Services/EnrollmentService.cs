using System.Net.Http.Json;
using SkillSprint.ClientUI.Models;

namespace SkillSprint.ClientUI.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(HttpClient httpClient, ILogger<EnrollmentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(int userId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CourseEnrollment>>($"api/Enrollments/user/{userId}");
            return result ?? new List<CourseEnrollment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get enrollments for user {UserId}", userId);
            return new List<CourseEnrollment>();
        }
    }

    public async Task<CourseEnrollment?> GetUserEnrollmentAsync(int userId, int courseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Enrollments/{userId}/course/{courseId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CourseEnrollment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get enrollment for user {UserId} and course {CourseId}", userId, courseId);
            return null;
        }
    }

    public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
    {
        var enrollment = await GetUserEnrollmentAsync(userId, courseId);
        return enrollment != null;
    }

    public async Task<CourseEnrollment?> EnrollAsync(int userId, int courseId)
    {
        try
        {
            var payload = new { UserId = userId, CourseId = courseId };
            var response = await _httpClient.PostAsJsonAsync("api/Enrollments", payload);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Enroll API returned {StatusCode} for user {UserId}, course {CourseId}",
                    response.StatusCode, userId, courseId);
                return null;
            }

            // API returns existing or created enrollment object
            return await response.Content.ReadFromJsonAsync<CourseEnrollment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enroll user {UserId} in course {CourseId}", userId, courseId);
            return null;
        }
    }

    public async Task<bool> UpdateProgressAsync(int enrollmentId, decimal progressPercentage)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Enrollments/{enrollmentId}/progress",
                new { ProgressPercentage = progressPercentage });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update progress for enrollment {EnrollmentId}", enrollmentId);
            return false;
        }
    }

    public async Task<bool> UnenrollAsync(int enrollmentId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Enrollments/{enrollmentId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unenroll for enrollment {EnrollmentId}", enrollmentId);
            return false;
        }
    }
}
