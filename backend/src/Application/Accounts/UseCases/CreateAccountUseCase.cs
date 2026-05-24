using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Domain.Entities;

namespace Application.Accounts.UseCases;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly Application.People.Interfaces.IPersonRepository _personRepository;

    public CreateAccountUseCase(
        IAccountRepository repository,
        Application.People.Interfaces.IPersonRepository personRepository)
    {
        _repository = repository;
        _personRepository = personRepository;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateAccountRequest request)
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

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Amount = request.Amount,
            DueDate = AccountRules.NormalizeToUtc(request.DueDate),
            Paid = false,
            Participants = participants.ToList(),
        };

        await _repository.CreateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
