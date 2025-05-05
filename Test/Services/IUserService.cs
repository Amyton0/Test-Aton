using Test.Models;

namespace Test.Services;

public interface IUserService
{
    Task<User?> Authenticate(string username, string password);
}