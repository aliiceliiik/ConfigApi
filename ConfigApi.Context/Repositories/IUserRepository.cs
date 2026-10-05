using ConfigApi.Entities.Dtos;

namespace ConfigApi.Context.Repositories;

public interface IUserRepository
{
    Task<UserDto?> GetByEmailAsync(string email);
}