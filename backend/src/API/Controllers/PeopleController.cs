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

    public PeopleController(
        GetPeopleUseCase getPeopleUseCase,
        CreatePersonUseCase createPersonUseCase)
    {
        _getPeopleUseCase = getPeopleUseCase;
        _createPersonUseCase = createPersonUseCase;
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
}