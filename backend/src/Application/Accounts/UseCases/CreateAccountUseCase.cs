using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.Auth.Interfaces;
using Domain.Entities;

namespace Application.Accounts.UseCases;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public CreateAccountUseCase(IAccountRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            TenantId = _currentUser.TenantId ?? Guid.Empty,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            DueDate = AccountRules.NormalizeToUtc(request.DueDate),
            CreatedAtUtc = DateTime.UtcNow,
            Paid = false,
            ParticipatesInDivision = request.ParticipatesInDivision,
        };

        await _repository.CreateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
