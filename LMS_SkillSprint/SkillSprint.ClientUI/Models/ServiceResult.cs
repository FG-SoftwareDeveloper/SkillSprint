using Microsoft.AspNetCore.Mvc;

namespace SkillSprint.ClientUI.Models
{
    public sealed class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public ProblemDetails? Problem { get; private set; }
        public T? Data { get; private set; }

        public static ServiceResult<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public static ServiceResult<T> Fail(string error, ProblemDetails? problem = null) =>
            new() { IsSuccess = false, Error = error, Problem = problem };
    }
}
