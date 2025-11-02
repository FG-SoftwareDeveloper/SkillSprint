using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillSprint.ClientUI.Models;
using SkillSprint.ClientUI.Services;
using System.Security.Claims;

namespace SkillSprint.ClientUI.Pages;

[Authorize]
public class CoursesModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesModel> _logger;

    public CoursesModel(ICourseService courseService, ILogger<CoursesModel> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public List<Course> Courses { get; set; } = new();
    public string? Message { get; set; }
    public string? SuccessMessage { get; set; }
    public int CurrentUserId { get; set; }

    public async Task OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            CurrentUserId = userId;
            Courses = await _courseService.GetAllCoursesWithEnrollmentStatusAsync(userId);
        }
        else
        {
            Courses = await _courseService.GetAllCoursesAsync();
        }

        _logger.LogInformation("Loaded {Count} courses", Courses.Count);
    }

    public async Task<IActionResult> OnPostEnrollAsync(int courseId)
    {
        _logger.LogInformation("OnPostEnrollAsync called with courseId: {CourseId}", courseId);
        
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            _logger.LogDebug("User claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

            if (userIdClaim == null)
            {
                _logger.LogWarning("User NameIdentifier claim not found");
                Message = "Unable to identify user. Please log in again.";
                await OnGetAsync();
                return Page();
            }

            _logger.LogDebug("Found NameIdentifier claim with value: {ClaimValue}", userIdClaim.Value);

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Failed to parse user ID from claim value: {ClaimValue}", userIdClaim.Value);
                Message = "Unable to identify user. Please log in again.";
                await OnGetAsync();
                return Page();
            }

            _logger.LogInformation("Attempting to enroll user {UserId} in course {CourseId}", userId, courseId);

            // Check if already enrolled before attempting enrollment
            var isAlreadyEnrolled = await _courseService.IsUserEnrolledAsync(userId, courseId);
            _logger.LogDebug("User {UserId} enrollment status for course {CourseId}: {IsEnrolled}", userId, courseId, isAlreadyEnrolled);

            if (isAlreadyEnrolled)
            {
                _logger.LogWarning("User {UserId} is already enrolled in course {CourseId}", userId, courseId);
                Message = "You are already enrolled in this course.";
                await OnGetAsync();
                return Page();
            }

            var success = await _courseService.EnrollInCourseAsync(userId, courseId);
            _logger.LogInformation("Enrollment service returned: {Success} for user {UserId} in course {CourseId}", success, userId, courseId);

            if (success)
            {
                SuccessMessage = "Successfully enrolled in course!";
                _logger.LogInformation("User {UserId} successfully enrolled in course {CourseId}", userId, courseId);
                
                // Verify enrollment was actually created
                var verifyEnrollment = await _courseService.IsUserEnrolledAsync(userId, courseId);
                _logger.LogDebug("Post-enrollment verification: User {UserId} enrolled in course {CourseId}: {IsEnrolled}", userId, courseId, verifyEnrollment);
                
                await OnGetAsync(); // Refresh the page data
                return Page();
            }
            else
            {
                Message = "Failed to enroll in course. Please try again.";
                _logger.LogError("Failed to enroll user {UserId} in course {CourseId} - service returned false", userId, courseId);
                await OnGetAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during enrollment for courseId {CourseId}", courseId);
            Message = "An error occurred while enrolling in the course. Please try again.";
            await OnGetAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUnenrollAsync(int enrollmentId, int courseId)
    {
        _logger.LogInformation("OnPostUnenrollAsync called with enrollmentId: {EnrollmentId}, courseId: {CourseId}", enrollmentId, courseId);
        
        try
        {
            if (enrollmentId <= 0)
            {
                _logger.LogWarning("Invalid enrollmentId provided: {EnrollmentId}", enrollmentId);
                Message = "Invalid enrollment information. Please refresh the page and try again.";
                await OnGetAsync();
                return Page();
            }

            // Get user ID for logging purposes
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int uid) ? uid : 0;
            
            _logger.LogInformation("Attempting to unenroll user {UserId} from enrollmentId {EnrollmentId} (courseId {CourseId})", userId, enrollmentId, courseId);

            var success = await _courseService.UnenrollFromCourseAsync(enrollmentId);
            _logger.LogInformation("Unenrollment service returned: {Success} for enrollmentId {EnrollmentId}", success, enrollmentId);

            if (success)
            {
                SuccessMessage = "Successfully unenrolled from course!";
                _logger.LogInformation("Successfully unenrolled from enrollment {EnrollmentId} (user {UserId}, course {CourseId})", enrollmentId, userId, courseId);
                
                // Verify unenrollment was successful
                if (userId > 0)
                {
                    var verifyUnenrollment = await _courseService.IsUserEnrolledAsync(userId, courseId);
                    _logger.LogDebug("Post-unenrollment verification: User {UserId} enrolled in course {CourseId}: {IsEnrolled}", userId, courseId, verifyUnenrollment);
                }
                
                await OnGetAsync(); // Refresh the page data
                return Page();
            }
            else
            {
                Message = "Failed to unenroll from course. Please try again.";
                _logger.LogError("Failed to unenroll from enrollment {EnrollmentId} - service returned false", enrollmentId);
                await OnGetAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during unenrollment for enrollmentId {EnrollmentId}", enrollmentId);
            Message = "An error occurred while unenrolling from the course. Please try again.";
            await OnGetAsync();
            return Page();
        }
    }
}