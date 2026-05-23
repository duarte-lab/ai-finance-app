using Application.Accounts.Interfaces;

namespace Application.Accounts.UseCases;

public class DeleteAccountUseCase
{
    private readonly IAccountRepository _repository;

    public DeleteAccountUseCase(IAccountRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(Guid id)
    {
        return _repository.DeleteAsync(id);
    }
}
