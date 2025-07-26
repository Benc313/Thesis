using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.UserService.Models;

namespace ThesisBackend.Application.UserService.Interfaces;

public interface IUserService
{
    Task<UserOperationResult> GetUser(int userId);
    Task<AllUserOperationResult> GetUsers();
    Task<UserOperationResult> UpdateUser(UserRequest userRequest, int userId);
    Task<UserEventOperationResult> GetUserMeets(int userId);
    Task<UserEventOperationResult> GetUserRaces(int userId);
}