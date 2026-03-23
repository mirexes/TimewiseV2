using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Users;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис управления пользователями
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<UserListDto>> GetAllAsync(UserFilterDto filter)
    {
        var query = _db.Users.AsQueryable();

        // Фильтр по активности
        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        // Фильтр по роли
        if (filter.Role.HasValue)
            query = query.Where(u => u.Role == filter.Role.Value);

        // Поиск по ФИО, телефону, email, компании
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.LastName.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                (u.MiddleName != null && u.MiddleName.ToLower().Contains(search)) ||
                u.Phone.Contains(search) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.Company != null && u.Company.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => u.ToListDto())
            .ToListAsync();

        return new PagedResultDto<UserListDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<UserDetailDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users
            .Include(u => u.Client)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user?.ToDetailDto();
    }

    public async Task<int> CreateAsync(CreateUserDto dto)
    {
        var user = new AppUser
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            Phone = dto.Phone,
            Email = dto.Email,
            Role = dto.Role,
            Company = dto.Company,
            ClientId = dto.ClientId,
            IsActive = dto.IsActive
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task UpdateAsync(int id, CreateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Пользователь {id} не найден");

        user.LastName = dto.LastName;
        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.Phone = dto.Phone;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.Company = dto.Company;
        user.ClientId = dto.ClientId;
        user.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateProfileAsync(int id, UpdateProfileDto dto)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Пользователь {id} не найден");

        user.LastName = dto.LastName;
        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.Email = dto.Email;
        user.Company = dto.Company;

        await _db.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Пользователь {id} не найден");

        user.IsActive = !user.IsActive;

        // При деактивации: понижаем роль до Клиент, убираем из чата компании, инвалидируем сессию
        if (!user.IsActive)
        {
            user.Role = UserRole.Client;
            user.SecurityStamp = Guid.NewGuid().ToString();

            // Удаляем из всех чатов компании
            var memberships = await _db.CompanyChatMembers
                .Where(m => m.UserId == id)
                .ToListAsync();
            _db.CompanyChatMembers.RemoveRange(memberships);
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAvatarAsync(int id, string? avatarUrl)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"Пользователь {id} не найден");

        user.AvatarUrl = avatarUrl;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null)
    {
        var query = _db.Users.Where(u => u.Phone == phone);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        return !await query.AnyAsync();
    }
}
