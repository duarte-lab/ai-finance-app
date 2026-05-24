using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.People.Interfaces;
using Domain.Entities;

namespace Application.Accounts.UseCases;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IPersonRepository _personRepository;

    public CreateAccountUseCase(
        IAccountRepository repository,
        IPersonRepository personRepository)
    {
        _repository = repository;
        _personRepository = personRepository;
    }

    public async Task<AccountResponse> ExecuteAsync(CreateAccountRequest request)
    {
        AccountRules.ValidateName(request.Name);
        AccountRules.ValidateAmount(request.Amount);
        var participants = AccountRules.BuildParticipants(request.Participants);
        await AccountRules.ValidateParticipantsExistAsync(_personRepository, participants);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Amount = request.Amount,
            DueDate = AccountRules.NormalizeToUtc(request.DueDate),
            CreatedAtUtc = DateTime.UtcNow,
            Paid = false,
            ParticipatesInDivision = request.ParticipatesInDivision,
            Participants = participants.ToList(),
        };

        await _repository.CreateAsync(account);
        return AccountMapper.ToResponse(account);
    }
}
