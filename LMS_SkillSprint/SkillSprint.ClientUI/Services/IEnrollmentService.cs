using SkillSprint.ClientUI.Models;
using System.Threading.Tasks;

namespace SkillSprint.ClientUI.Services;

public interface IEnrollmentService
{
    Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(int userId);
    Task<CourseEnrollment?> GetUserEnrollmentAsync(int userId, int courseId);
    Task<bool> IsUserEnrolledAsync(int userId, int courseId);
    Task<CourseEnrollment?> EnrollAsync(int userId, int courseId);
    Task<bool> UpdateProgressAsync(int enrollmentId, decimal progressPercentage);
    Task<bool> UnenrollAsync(int enrollmentId);
}
