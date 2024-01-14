using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Controllers.Models.Projects;
using ProjectManager.Api.Controllers.Models.Statuses;
using ProjectManager.Api.Controllers.Models.Todos;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;

namespace ProjectManager.Api.Controllers;
[Authorize]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly ILogger<TodoController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public StatusController(
        ILogger<TodoController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("api/v1/Status")]
    public async Task<ActionResult<IEnumerable<StatusDetailModel>>> GetList()
    {
        var dbEntities = await _dbContext
            .Set<Status>()
            .Include(x => x.Todos)
            .Include(x => x.Project)
            .FilterDeleted()
            .ToListAsync();

        return Ok(dbEntities.Select(x => x.ToDetail()));
    }

    [HttpGet("api/v1/Status/{id}")]
    public async Task<ActionResult<StatusDetailModel>> Get(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Status>()
            .Include(x => x.Todos)
            .Include(x => x.Project)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);

        return dbEntity != null ? Ok(dbEntity.ToDetail()) : NotFound();
    }

    [HttpPost("api/v1/Status")]
    public async Task<ActionResult> Create(
    [FromBody] StatusCreateModel sourceModel
    )
    {
        var now = _clock.GetCurrentInstant();
        var newEntity = new Status
        {
            Id = Guid.NewGuid(),
            Title = sourceModel.Title,
            ProjectId = sourceModel.ProjectId,
        }.SetCreateBySystem(now);
        var uniqueCheck = await _dbContext.Set<Status>().FilterDeleted().AnyAsync(x => x.Title == newEntity.Title);
        if (uniqueCheck)
        {
            ModelState.AddModelError<StatusCreateModel>(x => x.Title, "Title is NOT UNIQUE!");
        }
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        _dbContext.Add(newEntity);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpDelete("api/v1/Status/{id}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id
    )
    {
        var dbEntity = await _dbContext
            .Set<Status>()
            .Include(x => x.Todos)
            .Include(x => x.Project)
            .FilterDeleted()
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        if (dbEntity.Todos == Enumerable.Empty<Todo>())
        {
            return BadRequest();
        }
        dbEntity.SetDeleteBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
