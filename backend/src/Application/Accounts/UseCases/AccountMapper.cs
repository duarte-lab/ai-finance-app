using Application.Accounts.DTOs;
using Domain.Entities;

namespace Application.Accounts.UseCases;

internal static class AccountMapper
{
    public static AccountResponse ToResponse(Account account)
        => new(
            account.Id,
            account.Name,
            account.Amount,
            account.DueDate,
            account.Paid,
            account.Participants
                .Select(participant => new AccountParticipantResponse(
                    participant.PersonId,
                    participant.Percentage))
                .ToList());
}
