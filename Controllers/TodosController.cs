using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController(ITodoService service) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<TodoResponse>>> GetAll([FromQuery] bool? isCompleted)
  {
    var items = await service.GetAllAsync(isCompleted);
    return Ok(items);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<TodoResponse>> GetById(int id)
  {
    var item = await service.GetByIdAsync(id);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpPost]
  public async Task<ActionResult<TodoResponse>> Create([FromBody] CreateTodoDto dto)
  {
    var response = await service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<TodoResponse>> Update(int id, [FromBody] UpdateTodoDto dto)
  {
    var response = await service.UpdateAsync(id, dto);
    return response is null ? NotFound() : Ok(response);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var deleted = await service.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
  }

}
