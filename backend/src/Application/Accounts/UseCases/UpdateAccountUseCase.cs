using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class UpdateAccountUseCase
{
    private readonly IAccountRepository _repository;

    public UpdateAccountUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id, UpdateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);

        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return null;
        }

        account.Name = request.Name.Trim();
        account.Amount = request.Amount;
        account.DueDate = AccountRules.NormalizeToUtc(request.DueDate);
        account.Paid = request.Paid;
        account.ParticipatesInDivision = request.ParticipatesInDivision;

        await _repository.UpdateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
