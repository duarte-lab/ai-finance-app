using Application.Accounts.DTOs;
using Domain.Entities;

namespace Application.Accounts.UseCases;

internal static class AccountRules
{
    public static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
    }

    public static void ValidateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }
    }

    public static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public static IReadOnlyCollection<AccountParticipant> BuildParticipants(
        IReadOnlyCollection<AccountParticipantRequest>? participants)
    {
        if (participants is null || participants.Count == 0)
        {
            return [];
        }

        if (participants.Any(x => x.PersonId == Guid.Empty))
        {
            throw new ArgumentException("Participant person id is required.", nameof(participants));
        }

        if (participants.Any(x => x.Percentage <= 0))
        {
            throw new ArgumentException("Participant percentage must be greater than 0.", nameof(participants));
        }

        var hasDuplicates = participants
            .GroupBy(x => x.PersonId)
            .Any(group => group.Count() > 1);

        if (hasDuplicates)
        {
            throw new ArgumentException("A person cannot participate more than once.", nameof(participants));
        }

        var totalPercentage = participants.Sum(x => x.Percentage);
        if (decimal.Abs(totalPercentage - 100m) > 0.01m)
        {
            throw new ArgumentException("Participant percentages must add up to 100%.", nameof(participants));
        }

        return participants
            .Select(x => new AccountParticipant
            {
                PersonId = x.PersonId,
                Percentage = x.Percentage,
            })
            .ToList();
    }
}
