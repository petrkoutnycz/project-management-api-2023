using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Controllers.Models.Statuses;
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
    public async Task<ActionResult<IEnumerable<StatusDetailModel>>> GetList(
        [FromQuery] StatusFilter filter
        )
    {
        var dbEntities = await _dbContext
            .Set<Status>()
            .Include(x => x.Todos)
            .Include(x => x.Project)
            .FilterDeleted()
            .ApplyFilter(filter)
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

    // TODO: in REST, it is a common practice to send uri of created entity in headers back
    [HttpPost("api/v1/Status")]
    public async Task<ActionResult> Create(
    [FromBody] StatusCreateModel model
    )
    {
        var now = _clock.GetCurrentInstant();
        var newEntity = new Status
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            ProjectId = model.ProjectId,
        }.SetCreateBySystem(now);

        // TODO: business criteria implemented on infra level, nope please :-)
        // TODO: you know that strings are compared case sensitive here, right? Intentional?
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
