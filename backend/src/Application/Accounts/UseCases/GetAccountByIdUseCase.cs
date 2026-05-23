using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class GetAccountByIdUseCase
{
    private readonly IAccountRepository _repository;

    public GetAccountByIdUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);
        return account is null ? null : AccountMapper.ToResponse(account);
    }
}
