using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("closing")]
public class ClosingController : ControllerBase
{
    private readonly CreateMonthlyClosingUseCase _createMonthlyClosingUseCase;

    public ClosingController(CreateMonthlyClosingUseCase createMonthlyClosingUseCase)
    {
        _createMonthlyClosingUseCase = createMonthlyClosingUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMonthlyClosingRequest request)
    {
        try
        {
            var result = await _createMonthlyClosingUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
