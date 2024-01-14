using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using ProjectManager.Api.Controllers.Models.InnerModels;
using ProjectManager.Api.Controllers.Models.Projects;
using ProjectManager.Api.Controllers.Models.Statuses;
using ProjectManager.Api.Controllers.Models.Todos;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;

namespace ProjectManager.Api.Controllers;
[Authorize]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly ILogger<TodoController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public TodoController(
        ILogger<TodoController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("api/v1/Todo")]
    public async Task<ActionResult<IEnumerable<TodoDetailModel>>> GetList(
        [FromQuery] TodoFilter filter
        )
    {
        var query = _dbContext
            .Set<Todo>()
            .Include(x => x.Status)
            .FilterDeleted()
            .ApplyFilter(filter)
            ;

        var dbEntities = await query.ToListAsync();

        return Ok(dbEntities.Select(x => x.ToDetail()));
    }

    [HttpGet("api/v1/Todo/{id}")]
    public async Task<ActionResult<TodoDetailModel>> Get(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Todo>()
            .Include(x => x.Status)
            .FilterDeleted()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        return Ok(dbEntity.ToDetail());
    }

    [HttpPost("api/v1/Todo")]
    public async Task<ActionResult> Create(
        [FromBody] TodoCreateModel model
        )
    {
        var now = _clock.GetCurrentInstant();
        var newTodo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Description = model.Description,
            ProjectId = model.ProjectId,
            StatusId = model.StatusId,
        }.SetCreateBySystem(now);

        _dbContext.Add(newTodo);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch("api/v1/Todo/{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<TodoCreateModel> patch
        )
    {
        var dbEntity = await _dbContext
            .Set<Todo>()
            .FilterDeleted()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        var now = _clock.GetCurrentInstant();

        var toUpdate = new TodoCreateModel
        {
            Description = dbEntity.Description,
            Title = dbEntity.Title,
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        dbEntity.Title = toUpdate.Title;
        dbEntity.Description = toUpdate.Description;
        dbEntity.SetModifyBySystem(now);

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext.Set<Todo>().FirstAsync(x => x.Id == dbEntity.Id);

        return Ok(new TodoDetailModel
        {
            Id = dbEntity.Id,
            Description = dbEntity.Description,
            Title = dbEntity.Title,
            CreatedAt = InstantPattern.ExtendedIso.Format(dbEntity.CreatedAt),
            Status = new GenericInnerModel() { Id = dbEntity.StatusId, Name = dbEntity.Status.Title },
        });
    }

    [HttpDelete("api/v1/Todo/{id}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id
    )
    {
        var dbEntity = await _dbContext
            .Set<Todo>()
            .FilterDeleted()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        dbEntity.SetDeleteBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
