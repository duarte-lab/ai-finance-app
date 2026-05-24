using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.People.Interfaces;

namespace Application.Accounts.UseCases;

public class UpdateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IPersonRepository _personRepository;

    public UpdateAccountUseCase(
        IAccountRepository repository,
        IPersonRepository personRepository)
    {
        _repository = repository;
        _personRepository = personRepository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id, UpdateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);
        var participants = AccountRules.BuildParticipants(request.Participants);
        await AccountRules.ValidateParticipantsExistAsync(_personRepository, participants);

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
        account.Participants = participants.ToList();

        await _repository.UpdateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
