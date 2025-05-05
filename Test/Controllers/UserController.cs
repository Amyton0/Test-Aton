using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Test.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.Data;

namespace Test.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("current")]
    [Authorize] 
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
    
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Guid.ToString() == userId);
    
        if (user == null)
            return NotFound();
    
        return user;
    }
    
    [HttpPost("create")]
    [Authorize] 
    public async Task<IActionResult> CreateUser([FromBody] User dto)
    {
        if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
        {
            return BadRequest("Логин уже занят");
        }

        if (!Regex.IsMatch(dto.Login, @"^[a-zA-Z0-9]+$"))
            return BadRequest("Логин содержит недопустимые символы");
        if (!Regex.IsMatch(dto.Password, @"^[a-zA-Z0-9]+$"))
            return BadRequest("Пароль содержит недопустимые символы");
        if (!Regex.IsMatch(dto.Name, @"^[a-zA-Zа-яА-ЯёЁ]+$"))
            return BadRequest("Имя содержит недопустимые символы");
        
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest("Пользователь не найден");
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest("Пользователь не найден");

        if (!user.Admin && dto.Admin)
            return BadRequest("Для этого действия нужны права админстратора");

        await _context.Users.AddAsync(dto);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPut("users/{id}/update")]
    [Authorize] 
    public async Task<IActionResult> UpdateUserProfile(
        Guid id, 
        [FromBody] UserProfileUpdateDto dto)
    {
        if (!Regex.IsMatch(dto.Name, @"^[a-zA-Zа-яА-ЯёЁ]+$"))
            return BadRequest("Имя содержит недопустимые символы");
        
        var currentUserResult = await GetCurrentUser();
        if (currentUserResult.Result is NotFoundResult || currentUserResult.Value == null)
            return BadRequest("Текущий пользователь не найден");
    
        var currentUser = currentUserResult.Value;

        if (currentUser.Guid != id && !currentUser.Admin)
            return BadRequest("Вы не можете изменить данные этого пользователя");

        if (currentUser.Guid == id && currentUser.RevokedOn != default)
            return BadRequest("Ваш аккаунт удалён");

        var userToUpdate = await _context.Users.FindAsync(id);
        if (userToUpdate == null)
            return NotFound();

        if (dto.Name != null)
            userToUpdate.Name = dto.Name;
    
        if (dto.Gender.HasValue)
            userToUpdate.Gender = dto.Gender.Value;
    
        if (dto.Birthday.HasValue)
            userToUpdate.Birthday = dto.Birthday.Value;

        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPut("users/{id}/changepassword")]
    [Authorize] 
    public async Task<IActionResult> UpdatePassword(
        Guid id, 
        [FromBody] string password)
    {
        if (!Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
            return BadRequest("Пароль содержит недопустимые символы");
        
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (user.Guid != id && !user.Admin)
            return BadRequest("Вы не можете поменять данные этого пользователя");

        if (user.Guid == id && user.RevokedOn != null)
        {
            return BadRequest("Ваш аккаунт удалён, вы не можете изменить его данные");
        }
        
        var userToUpdate = await _context.Users.FindAsync(id);
    
        if (userToUpdate == null)
            return NotFound();

        userToUpdate.Password = password;
    
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPut("users/{id}/changelogin")]
    [Authorize] 
    public async Task<IActionResult> UpdateLogin(
        Guid id, 
        [FromBody] string login)
    {
        if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
            return BadRequest("Логин содержит недопустимые символы");
        
        if (await _context.Users
                .AnyAsync(u => u.Login == login && u.Guid != id))
            return BadRequest("Логин уже занят другим пользователем");
        
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (user.Guid != id && !user.Admin)
            return BadRequest("Вы не можете поменять данные этого пользователя");

        if (user.Guid == id && user.RevokedOn != null)
        {
            return BadRequest("Ваш аккаунт удалён, вы не можете изменить его данные");
        }
        
        var userToUpdate = await _context.Users.FindAsync(id);
    
        if (userToUpdate == null)
            return NotFound();

        userToUpdate.Login = login;
    
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPut("users/{id}/restore")]
    [Authorize] 
    public async Task<IActionResult> RestoreUser(
        Guid id)
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (!user.Admin)
            return BadRequest("Это действие доступно только админам");
        
        var userToUpdate = await _context.Users.FindAsync(id);
    
        if (userToUpdate == null)
            return NotFound();

        userToUpdate.RevokedOn = null;
        userToUpdate.RevokedBy = "";
    
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("users")]
    [Authorize] 
    public async Task<ActionResult<IQueryable<User>>> GetUsers()
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (!user.Admin)
            return BadRequest("Это действие доступно только админам");
        
        return Ok(_context.Users.Where(x => x.RevokedOn == default).OrderBy(x => x.CreatedOn));
    }

    [HttpGet("user/{login}")]
    [Authorize] 
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (!user.Admin)
            return BadRequest("Это действие доступно только админам");
        
        var userToGet = _context.Users.FirstOrDefault(x => x.Login == login);

        if (userToGet == null)
            return NotFound();

        return Ok(new
        {
            Name = userToGet.Name,
            Gender = userToGet.Gender,
            Birthday = userToGet.Birthday,
            isActive = userToGet.RevokedOn == default
        });
    }
    
    [HttpGet("user")]
    [Authorize] 
    public async Task<IActionResult> GetUser(
        [FromQuery] string login, 
        [FromQuery] string password)
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (user.Login != login)
            return BadRequest("Это действие вам недоступно");

        var userToGet = await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
        if (userToGet == null || userToGet.Password != password)
        {
            return Unauthorized("Неверные учетные данные");
        }

        return Ok(userToGet);
    }
    
    [HttpGet("users/{age}")]
    [Authorize] 
    public async Task<IActionResult> GetUsersByAge(int age)
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (!user.Admin)
            return BadRequest("Это действие доступно только админам");
        
        var users = _context.Users.Where(x => x.Birthday < DateTime.Today.AddYears(-age));

        return Ok(users);
    }
    
    [HttpDelete("user/{id}/{isFull}")]
    [Authorize] 
    public async Task<IActionResult> DeleteUser(Guid id, bool isFull)
    {
        var result = await GetCurrentUser();
        if (result.Result is NotFoundResult) 
            return BadRequest();
    
        var user = result.Value;
        
        if (user == null)
            return BadRequest();

        if (!user.Admin)
            return BadRequest("Это действие доступно только админам");
        
        var userToDelete = await _context.Users.FindAsync(id);
    
        if (userToDelete == null)
            return NotFound();

        if (!isFull)
        {
            userToDelete.RevokedOn = DateTime.Now;
            userToDelete.RevokedBy = user.Login;
        }
        else
        {
            _context.Users.Remove(userToDelete);
        }
        await _context.SaveChangesAsync();

        return Ok();
    }
}