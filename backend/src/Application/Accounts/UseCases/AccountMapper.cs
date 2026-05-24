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
            account.CreatedAtUtc,
            account.Paid,
            account.ParticipatesInDivision);
}
