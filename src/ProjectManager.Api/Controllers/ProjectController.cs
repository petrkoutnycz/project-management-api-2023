using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using ProjectManager.Api.Controllers.Models.Projects;
using ProjectManager.Api.Controllers.Models.Statuses;
using ProjectManager.Api.Controllers.Models.Todos;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;

namespace ProjectManager.Api.Controllers;
[Authorize]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly ILogger<TodoController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public ProjectController(
        ILogger<TodoController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("api/v1/Project")]
    public async Task<ActionResult<IEnumerable<ProjectDetailModel>>> GetList()
    {
        var dbEntities = await _dbContext
            .Set<Project>()
            .Include(x => x.Todos)
            .Include(x => x.Statuses)
            .FilterDeleted()
            .ToListAsync();

        return Ok(dbEntities.Select(x => x.ToDetail()));
    }

    [HttpGet("api/v1/Project/{id}")]
    public async Task<ActionResult<ProjectDetailModel>> Get(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Project>()
            .Include(x => x.Todos)
            .Include(x => x.Statuses)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        var result = new ProjectDetailModel
        {
            Id = dbEntity.Id,
            Title = dbEntity.Title,
            Description = dbEntity.Description,
            CreatedAt = InstantPattern.ExtendedIso.Format(dbEntity.CreatedAt),
            Todos = dbEntity.Todos.Select(y => new TodoDetailModel
            {
                Id = y.Id,
                Title = y.Title,
                Description = y.Description,
                CreatedAt = InstantPattern.ExtendedIso.Format(y.CreatedAt),
            }),
            Statuses = dbEntity.Statuses.Select(x => x.ToDetail()),
        };

        return Ok(result);
    }

    [HttpPost("api/v1/Project")]
    public async Task<ActionResult> Create(
        [FromBody] ProjectCreateModel model
        )
    {
        var now = _clock.GetCurrentInstant();

        var newProject = new Project
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Description = model.Description,
        }.SetCreateBySystem(now);

        var uniqueCheck = await _dbContext.Set<Project>().AnyAsync(x => x.Title == newProject.Title);

        if (uniqueCheck)
        {
            ModelState.AddModelError<ProjectCreateModel>(x => x.Title, "title is not unique");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _dbContext.Add(newProject);
        await _dbContext.SaveChangesAsync();

        var dbEntity = await _dbContext.Set<Project>().FirstAsync(x => x.Id == newProject.Id);

        var url = Url.Action(nameof(Get), new { dbEntity.Id }) ?? throw new Exception("failed to generate url");
        return Created(url, dbEntity.ToDetail());
    }

    [HttpPatch("api/v1/Project/{id}")]
    public async Task<ActionResult<ProjectDetailModel>> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<ProjectCreateModel> patch)
    {
        var dbEntity = await _dbContext.Set<Project>().FirstOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        var toUpdate = dbEntity.ToUpdate();

        patch.ApplyTo(toUpdate);

        var uniqueCheck = await _dbContext.Set<Project>().AnyAsync(x => x.Id != id && x.Title == toUpdate.Title);

        if (uniqueCheck)
        {
            ModelState.AddModelError<ProjectCreateModel>(x => x.Title, "title is not unique");
        }

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        dbEntity.Title = toUpdate.Title;
        dbEntity.Description = toUpdate.Description;

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext.Set<Project>().FirstAsync(x => x.Id == id);
        return Ok(dbEntity.ToDetail());
    }

    [HttpGet("api/v1/Project/error")]
    public async Task<ActionResult> Error()
    {
        throw new NotImplementedException();
    }
}
