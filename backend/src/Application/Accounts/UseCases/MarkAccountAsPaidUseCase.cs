using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class MarkAccountAsPaidUseCase
{
    private readonly IAccountRepository _repository;

    public MarkAccountAsPaidUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return null;
        }

        account.MarkAsPaid();
        await _repository.UpdateAsync(account);

        return AccountMapper.ToResponse(account);
    }
}
