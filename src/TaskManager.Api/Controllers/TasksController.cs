using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController(ITaskService service, IMapper mapper) : ControllerBase
    {
        private readonly ITaskService _service = service ?? throw new ArgumentNullException(nameof(service));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _service.GetAllAsync();
            if (result.IsSuccess && result.Value is not null)
                return Ok(result.Value.Select(_mapper.Map<TaskResponseDto>));

            return StatusCode(500, result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result.IsSuccess && result.Value is not null)
                return Ok(_mapper.Map<TaskResponseDto>(result.Value));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.InvariantCultureIgnoreCase)))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TaskCreateDto dto)
        {
            var entity = _mapper.Map<TaskItem>(dto);
            var result = await _service.CreateAsync(entity);

            if (result.IsSuccess && result.Value is not null)
                return CreatedAtAction(
                    nameof(GetByIdAsync),
                    new { id = result.Value.Id },
                    _mapper.Map<TaskResponseDto>(result.Value));

            if (result.Errors.Any(e => e.Contains("validation", StringComparison.InvariantCultureIgnoreCase)))
                return BadRequest(result.Errors);

            return StatusCode(500, result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] TaskUpdateDto dto)
        {
            var mapped = _mapper.Map<TaskItem>(dto);
            var updatedTask = new TaskItem
            {
                Id = id,
                Title = mapped.Title,
                Description = mapped.Description,
                Status = mapped.Status,
                Priority = mapped.Priority,
                CreatedAt = mapped.CreatedAt,
                DueDate = mapped.DueDate,
                CompletedAt = mapped.CompletedAt,
                UpdatedAt = mapped.UpdatedAt,
                IsDeleted = mapped.IsDeleted
            };

            var result = await _service.UpdateAsync(updatedTask);

            if (result.IsSuccess)
                return Ok(_mapper.Map<TaskResponseDto>(result.Value));

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
