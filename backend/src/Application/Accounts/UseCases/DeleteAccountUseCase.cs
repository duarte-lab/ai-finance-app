using Application.Accounts.Interfaces;
using Application.MonthlyClosing.Interfaces;

namespace Application.Accounts.UseCases;

public class DeleteAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IMonthlyClosingRepository _monthlyClosingRepository;

    public DeleteAccountUseCase(IAccountRepository repository, IMonthlyClosingRepository monthlyClosingRepository)
    {
        _repository = repository;
        _monthlyClosingRepository = monthlyClosingRepository;
    }

    public async Task<bool> ExecuteAsync(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return false;
        }

        var activeClosing = await _monthlyClosingRepository.GetActiveByYearMonthAsync(
            account.DueDate.Year, account.DueDate.Month);

        if (activeClosing is not null)
        {
            throw new InvalidOperationException("Cannot delete an account from a closed month.");
        }

        return await _repository.DeleteAsync(id);
    }
}
