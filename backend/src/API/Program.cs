using System.Text;
using Application.Accounts.Interfaces;
using Application.Accounts.UseCases;
using Application.Dashboard.UseCases;
using Application.MonthlyClosing.Interfaces;
using Application.MonthlyClosing.UseCases;
using Application.Notifications.Interfaces;
using Application.Notifications.UseCases;
using Application.People.Interfaces;
using Application.People.UseCases;
using Api.Notifications;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IMonthlyClosingRepository, MonthlyClosingRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<GetAccountsUseCase>();
builder.Services.AddScoped<GetAccountByIdUseCase>();
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<UpdateAccountUseCase>();
builder.Services.AddScoped<DeleteAccountUseCase>();
builder.Services.AddScoped<MarkAccountAsPaidUseCase>();
builder.Services.AddScoped<GetDashboardSummaryUseCase>();
builder.Services.AddScoped<CreateMonthlyClosingUseCase>();
builder.Services.AddScoped<GetPeopleUseCase>();
builder.Services.AddScoped<CreatePersonUseCase>();
builder.Services.AddScoped<GetDueNotificationsUseCase>();
builder.Services.AddSingleton<INotificationClock, SystemNotificationClock>();
builder.Services.AddHostedService<NotificationsBackgroundService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("SUPER_SECRET_KEY_123"))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendPolicy");
app.MapControllers();

app.Run();

public partial class Program;
