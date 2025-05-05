using Test.Models;

namespace Test.Data;

public static class DefaultData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Guid = new Guid(), 
                    Login = "Admin", 
                    Password = "admin",
                    Name = "Admin",
                    Gender = 0,
                    Admin = true,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now
                }
            );
            
            context.SaveChanges();
        }
    }
}