using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class UpdateAccountDivisionParticipationUseCase
{
    private readonly IAccountRepository _repository;

    public UpdateAccountDivisionParticipationUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id, bool participatesInDivision)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return null;
        }

        account.ParticipatesInDivision = participatesInDivision;

        await _repository.UpdateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}