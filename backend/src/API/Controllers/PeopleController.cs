using Application.People.DTOs;
using Application.People.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly GetPeopleUseCase _getPeopleUseCase;
    private readonly CreatePersonUseCase _createPersonUseCase;
    private readonly UpdatePersonUseCase _updatePersonUseCase;
    private readonly DeletePersonUseCase _deletePersonUseCase;

    public PeopleController(
        GetPeopleUseCase getPeopleUseCase,
        CreatePersonUseCase createPersonUseCase,
        UpdatePersonUseCase updatePersonUseCase,
        DeletePersonUseCase deletePersonUseCase)
    {
        _getPeopleUseCase = getPeopleUseCase;
        _createPersonUseCase = createPersonUseCase;
        _updatePersonUseCase = updatePersonUseCase;
        _deletePersonUseCase = deletePersonUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var people = await _getPeopleUseCase.ExecuteAsync();
        return Ok(people);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequest request)
    {
        try
        {
            var person = await _createPersonUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(Get), new { id = person.Id }, person);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePersonRequest request)
    {
        try
        {
            var person = await _updatePersonUseCase.ExecuteAsync(id, request);
            return Ok(person);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _deletePersonUseCase.ExecuteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}