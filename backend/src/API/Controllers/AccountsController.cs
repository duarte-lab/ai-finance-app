using Application.Accounts.DTOs;
using Application.Accounts.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly GetAccountsUseCase _getAccountsUseCase;
    private readonly GetAccountByIdUseCase _getAccountByIdUseCase;
    private readonly CreateAccountUseCase _createAccountUseCase;
    private readonly UpdateAccountUseCase _updateAccountUseCase;
    private readonly UpdateAccountDivisionParticipationUseCase _updateAccountDivisionParticipationUseCase;
    private readonly DeleteAccountUseCase _deleteAccountUseCase;
    private readonly MarkAccountAsPaidUseCase _markAccountAsPaidUseCase;

    public AccountsController(
        GetAccountsUseCase getAccountsUseCase,
        GetAccountByIdUseCase getAccountByIdUseCase,
        CreateAccountUseCase createAccountUseCase,
        UpdateAccountUseCase updateAccountUseCase,
        UpdateAccountDivisionParticipationUseCase updateAccountDivisionParticipationUseCase,
        DeleteAccountUseCase deleteAccountUseCase,
        MarkAccountAsPaidUseCase markAccountAsPaidUseCase)
    {
        _getAccountsUseCase = getAccountsUseCase;
        _getAccountByIdUseCase = getAccountByIdUseCase;
        _createAccountUseCase = createAccountUseCase;
        _updateAccountUseCase = updateAccountUseCase;
        _updateAccountDivisionParticipationUseCase = updateAccountDivisionParticipationUseCase;
        _deleteAccountUseCase = deleteAccountUseCase;
        _markAccountAsPaidUseCase = markAccountAsPaidUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        if ((year.HasValue && !month.HasValue) || (!year.HasValue && month.HasValue))
        {
            return BadRequest("When filtering by month, both year and month are required.");
        }

        if (month is < 1 or > 12)
        {
            return BadRequest("Month must be between 1 and 12.");
        }

        if (year <= 0)
        {
            return BadRequest("Year must be greater than 0.");
        }

        var items = await _getAccountsUseCase.ExecuteAsync(year, month);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _getAccountByIdUseCase.ExecuteAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
    {
        try
        {
            var account = await _createAccountUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request)
    {
        try
        {
            var account = await _updateAccountUseCase.ExecuteAsync(id, request);
            return account is null ? NotFound() : Ok(account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/pay")]
    public async Task<IActionResult> MarkAsPaid(Guid id)
    {
        var account = await _markAccountAsPaidUseCase.ExecuteAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPatch("{id:guid}/division-participation")]
    public async Task<IActionResult> UpdateDivisionParticipation(Guid id, [FromBody] UpdateDivisionParticipationRequest request)
    {
        var account = await _updateAccountDivisionParticipationUseCase.ExecuteAsync(id, request.ParticipatesInDivision);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _deleteAccountUseCase.ExecuteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}