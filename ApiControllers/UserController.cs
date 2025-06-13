using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/user/")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    
    public UserController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("add-credits")]
    public async Task<IActionResult> AddCredits([FromQuery] Guid userId, [FromQuery] int credits)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return BadRequest("User not found");
        }
        
        user.Credits += credits;
        await _dbContext.SaveChangesAsync();
        
        return Ok("Success");
    }

    [HttpGet("get-credits")]
    public async Task<IActionResult> GetCredits([FromQuery] Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            var response = new
            {
                credits = 0
            };

            return Ok(response);
        }

        var result = new
        {
            credits = user.Credits
        };
            
        return Ok(result);
    }
    
    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();
            
            var payload = JsonSerializer.Deserialize<RegisterUserPayload>(requestBody);

            if (payload == null)
            {
                return BadRequest("Invalid request body");
            }
            
            var user = new User
            {
                Id = payload.Id,
                FcmTokenId = payload.FcmTokenId,
                Gender = payload.Gender,
                TuneId = 0,
                Credits = 0
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var response = new
            {
                userId = user.Id
            };
            
            return Ok("User created");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}