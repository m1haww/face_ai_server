using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using System.Linq;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/job/")]
public class JobController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public JobController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet("list")]
    public async Task<IActionResult> GetUserImages([FromQuery] Guid userId)
    {
        var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (foundUser == null)
        {
            return Ok(new List<ImageJob>());
        }

        var jobs = await _dbContext.ImageJobs
            .Where(j => j.UserId == userId)
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("list-job-images")]
    public async Task<IActionResult> GetJobImages([FromQuery] int jobId)
    {
        var foundJob = await _dbContext.ImageJobs.FirstOrDefaultAsync(j => j.Id == jobId);

        if (foundJob == null)
        {
            return Ok("[]");
        }

        var images = foundJob.Images;
        
        return Ok(images);
    }

    [HttpGet("list-enhance-job-images")]
    public async Task<IActionResult> GetJobImages([FromQuery] string jobId)
    {
        var images = await _dbContext.EnhanceImages.Where(i => i.JobId == jobId).ToListAsync();
        
        return Ok(images);
    }
    
    [HttpGet("list-enhance-jobs")]
    public async Task<IActionResult> GetEnhanceJobs([FromQuery] Guid userId)
    {
        var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (foundUser == null)
        {
            return Ok(new List<EnhanceJob>());
        }
        
        var jobs = await _dbContext.EnhanceJobs
            .Where(j => j.UserId == userId).ToListAsync();
        
        return Ok(jobs);
    }

    [HttpGet("list-all-jobs")]
    public async Task<IActionResult> GetAllUserJobs([FromQuery] Guid userId)
    {
        var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (foundUser == null)
        {
            return Ok(new 
            {
                imageJobs = new List<object>(),
                enhanceJobs = new List<object>(),
                runwayJobs = new List<object>()
            });
        }

        var imageJobs = await _dbContext.ImageJobs
            .Where(j => j.UserId == userId && j.PresetCategory != PresetCategory.RunwayGenerated)
            .OrderByDescending(j => j.CreationDate)
            .ToListAsync();

        var enhanceJobs = await _dbContext.EnhanceJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
        
        var runwayJobs = await _dbContext.RunwayImageJobs
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            imageJobs = imageJobs,
            enhanceJobs = enhanceJobs,
            runwayJobs = runwayJobs,
        });
    }
}