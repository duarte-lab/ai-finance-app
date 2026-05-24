using Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infrastructure.Data;

public class AppDbContext
{
    private readonly IMongoDatabase _database;

    public AppDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("MongoDb connection string is not configured.");

        var databaseName = configuration["MongoDb:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDb database name is not configured.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Account> Accounts => _database.GetCollection<Account>("Accounts");
    public IMongoCollection<MonthlyClosing> MonthlyClosings => _database.GetCollection<MonthlyClosing>("MonthlyClosings");
    public IMongoCollection<Person> People => _database.GetCollection<Person>("People");
}