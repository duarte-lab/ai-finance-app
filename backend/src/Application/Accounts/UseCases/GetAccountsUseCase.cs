using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class GetAccountsUseCase
{
    private readonly IAccountRepository _repository;

    public GetAccountsUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<AccountResponse>> ExecuteAsync(int? year = null, int? month = null)
    {
        var accounts = await _repository.GetAllAsync(year, month);
        return accounts.Select(AccountMapper.ToResponse).ToList();
    }
}
