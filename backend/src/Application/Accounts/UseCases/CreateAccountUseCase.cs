using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Domain.Entities;

namespace Application.Accounts.UseCases;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _repository;

    public CreateAccountUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Amount = request.Amount,
            DueDate = AccountRules.NormalizeToUtc(request.DueDate),
            Paid = false
        };

        await _repository.CreateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
