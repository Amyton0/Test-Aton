using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Models;

namespace Test.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }
    
    
    public async Task<User?> Authenticate(string login, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        
        if (user == null || user.Password != password)
            return null;

        return user;
    }
}