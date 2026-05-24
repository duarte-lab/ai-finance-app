using Application.People.Interfaces;

namespace Application.People.UseCases;

public class DeletePersonUseCase
{
    private static readonly TimeSpan HardDeleteGracePeriod = TimeSpan.FromDays(30);
    private readonly IPersonRepository _repository;

    public DeletePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id)
    {
        var person = await _repository.GetByIdAsync(id);
        if (person is null)
        {
            throw new KeyNotFoundException("Person not found.");
        }

        if (person.DeletedAtUtc is null)
        {
            person.DeletedAtUtc = DateTime.UtcNow;
            await _repository.UpdateAsync(person);
            return;
        }

        if (person.DeletedAtUtc.Value.Add(HardDeleteGracePeriod) <= DateTime.UtcNow)
        {
            await _repository.DeleteAsync(id);
            return;
        }

        throw new InvalidOperationException("Person can only be permanently removed after 30 days from deletion date.");
    }
}
