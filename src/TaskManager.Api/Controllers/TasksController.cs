using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Entities;
using TaskManager.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(ITaskService service) : ControllerBase
    {
        private readonly ITaskService _service = service ?? throw new ArgumentNullException(nameof(service));

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(500, result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result.IsSuccess && result.Value is not null)
                return Ok(result.Value);

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.InvariantCultureIgnoreCase)))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TaskItem taskItem)
        {
            var result = await _service.CreateAsync(taskItem);

            if (result.IsSuccess && result.Value is not null)
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id }, result.Value);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] TaskItem taskItem)
        {
            var updatedTask = new TaskItem
            {
                Id = id,
                CompletedAt = taskItem.CompletedAt,
                CreatedAt = taskItem.CreatedAt,
                Description = taskItem.Description,
                DueDate = taskItem.DueDate,
                IsDeleted = taskItem.IsDeleted,
                Priority = taskItem.Priority,
                Status = taskItem.Status,
                Title = taskItem.Title,
                UpdatedAt = taskItem.UpdatedAt
            };

            var result = await _service.UpdateAsync(updatedTask);

            if (result.IsSuccess)
                return Ok(result.Value);

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.InvariantCultureIgnoreCase)))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _service.DeleteAsync(id);

            if (result.IsSuccess)
                return NoContent();

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.InvariantCultureIgnoreCase)))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreAsync(Guid id)
        {
            var result = await _service.RestoreAsync(id);

            if (result.IsSuccess)
                return Ok();

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.InvariantCultureIgnoreCase)))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }
    }
}
