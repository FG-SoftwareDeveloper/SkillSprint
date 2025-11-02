    using System.Threading.Tasks;
using SkillSprint.ClientUI.Models;

namespace SkillSprint.ClientUI.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponse>> LoginAsync(string email, string password);
    }
}
