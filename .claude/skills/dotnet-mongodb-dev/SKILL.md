---
name: dotnet-mongodb-dev
description: "Use when developing .NET 9 backend features with MongoDB, Clean Architecture, and unit tests. Triggers on: creating entities, repositories, use cases, DTOs, controllers, AppDbContext collections, xUnit tests, Moq mocks, integration tests, MongoDB driver usage, service registration, REST endpoints."
argument-hint: "Describe the feature or test to implement (e.g. 'create Transaction entity with CRUD and unit tests')"
---

# .NET 9 + MongoDB + Unit Tests Development

## Stack
- .NET 9 / C# 13
- MongoDB.Driver (IMongoCollection, IMongoDatabase)
- xUnit + Moq for unit tests
- Clean Architecture (Domain → Application → Infrastructure → API)

## Architecture Layers

```
Domain/          → Entities, value objects, no external dependencies
Application/     → Use cases, interfaces, DTOs, no infrastructure dependencies
Infrastructure/  → MongoDB repositories, AppDbContext, external services
API/             → Controllers, DI registration, Program.cs
Tests/           → Mirror src/ structure, one test project per layer
```

**Rule: layers only depend inward. API → Application → Domain. Infrastructure implements Application interfaces.**

## Procedure

### 1. Domain Entity
- Plain C# class, no framework attributes
- Use `Guid` for Id
- Use UTC for all `DateTime` fields (`DateTime.UtcNow`)
- `required` keyword for mandatory string fields

```csharp
namespace Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 2. DTO (Application layer)
- One DTO per operation direction: `CreateXxxRequest`, `XxxResponse`
- Never expose domain entities directly from controllers

```csharp
namespace Application.DTOs;

public record CreateTransactionRequest(string Description, decimal Amount);
public record TransactionResponse(Guid Id, string Description, decimal Amount, DateTime CreatedAt);
```

### 3. Repository Interface (Application layer)
```csharp
namespace Application.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<Transaction?> GetByIdAsync(Guid id);
    Task CreateAsync(Transaction transaction);
}
```

### 4. MongoDB Repository (Infrastructure layer)
- Inject `AppDbContext`
- Use async MongoDB driver methods
- Map domain entities — do not store DTOs in MongoDB

```csharp
namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _collection;

    public TransactionRepository(AppDbContext context)
        => _collection = context.Transactions;

    public async Task<IEnumerable<Transaction>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Transaction transaction)
        => await _collection.InsertOneAsync(transaction);
}
```

### 5. AppDbContext collection registration (Infrastructure/Data/AppDbContext.cs)
Add a new `IMongoCollection<T>` property for each new entity:

```csharp
public IMongoCollection<Transaction> Transactions
    => _database.GetCollection<Transaction>("Transactions");
```

### 6. Use Case (Application layer)
```csharp
namespace Application.UseCases;

public class CreateTransactionUseCase
{
    private readonly ITransactionRepository _repository;

    public CreateTransactionUseCase(ITransactionRepository repository)
        => _repository = repository;

    public async Task<TransactionResponse> ExecuteAsync(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            Amount = request.Amount
        };

        await _repository.CreateAsync(transaction);

        return new TransactionResponse(transaction.Id, transaction.Description, transaction.Amount, transaction.CreatedAt);
    }
}
```

### 7. Controller (API layer)
- `[ApiController]` + `[Route("api/[controller]")]`
- Return `IActionResult`; use `Ok()`, `NotFound()`, `BadRequest()`
- Inject use cases, not repositories

```csharp
namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly CreateTransactionUseCase _createUseCase;

    public TransactionsController(CreateTransactionUseCase createUseCase)
        => _createUseCase = createUseCase;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionRequest request)
        => Ok(await _createUseCase.ExecuteAsync(request));
}
```

### 8. DI Registration (Program.cs)
```csharp
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<CreateTransactionUseCase>();
```

---

## Unit Tests

### Test Project Setup
- One test project per src project: `Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`
- Framework: **xUnit**
- Mocking: **Moq**
- Naming: `MethodName_Scenario_ExpectedBehavior`

```xml
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
```

### Use Case Test (Application.Tests)
Mock the repository interface, test business logic in isolation:

```csharp
public class CreateTransactionUseCaseTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock = new();
    private readonly CreateTransactionUseCase _sut;

    public CreateTransactionUseCaseTests()
        => _sut = new CreateTransactionUseCase(_repositoryMock.Object);

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsResponse()
    {
        var request = new CreateTransactionRequest("Salary", 1500m);

        var result = await _sut.ExecuteAsync(request);

        result.Description.Should().Be("Salary");
        result.Amount.Should().Be(1500m);
        result.Id.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Transaction>()), Times.Once);
    }
}
```

### Repository Test (Infrastructure.Tests)
Use an in-memory MongoDB runner or mock `IMongoCollection<T>`:

```csharp
public class TransactionRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ValidTransaction_InsertsOnce()
    {
        var collectionMock = new Mock<IMongoCollection<Transaction>>();
        // Setup and assert InsertOneAsync called once
    }
}
```

---

## Rules Checklist

- [ ] UTC for all dates
- [ ] DTOs for all API input/output (never expose entities)
- [ ] Repository interfaces in Application, implementations in Infrastructure
- [ ] Use cases inject interfaces (not concrete repos)
- [ ] Controllers inject use cases (not repositories)
- [ ] Every new feature has unit tests
- [ ] `Guid.NewGuid()` assigned in use case, not controller
- [ ] Connection strings from `IConfiguration`, never hardcoded
