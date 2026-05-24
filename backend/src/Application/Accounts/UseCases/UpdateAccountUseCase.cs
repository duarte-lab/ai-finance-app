using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class UpdateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly Application.People.Interfaces.IPersonRepository _personRepository;

    public UpdateAccountUseCase(
        IAccountRepository repository,
        Application.People.Interfaces.IPersonRepository personRepository)
    {
        _repository = repository;
        _personRepository = personRepository;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid id, UpdateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);
        var participants = AccountRules.BuildParticipants(request.Participants);

        if (participants.Count > 0)
        {
            var people = await _personRepository.GetByIdsAsync(participants.Select(x => x.PersonId).ToList());
            if (people.Count != participants.Count)
            {
                throw new ArgumentException("All participants must reference existing people.", nameof(request));
            }
        }

        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return null;
        }

        account.Name = request.Name.Trim();
        account.Amount = request.Amount;
        account.DueDate = AccountRules.NormalizeToUtc(request.DueDate);
        account.Paid = request.Paid;
        account.Participants = participants.ToList();

        await _repository.UpdateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
