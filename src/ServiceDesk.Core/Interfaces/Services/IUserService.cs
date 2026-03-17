using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Users;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис управления пользователями
/// </summary>
public interface IUserService
{
    Task<PagedResultDto<UserListDto>> GetAllAsync(UserFilterDto filter);
    Task<UserDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateUserDto dto);
    Task UpdateAsync(int id, CreateUserDto dto);
    Task ToggleActiveAsync(int id);
    Task UpdateAvatarAsync(int id, string? avatarUrl);
    Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null);
}
