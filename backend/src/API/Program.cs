using System.Text;
using Application.Accounts.Interfaces;
using Application.Accounts.UseCases;
using Application.Dashboard.UseCases;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
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
builder.Services.AddScoped<GetAccountsUseCase>();
builder.Services.AddScoped<GetAccountByIdUseCase>();
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<UpdateAccountUseCase>();
builder.Services.AddScoped<DeleteAccountUseCase>();
builder.Services.AddScoped<MarkAccountAsPaidUseCase>();
builder.Services.AddScoped<GetDashboardSummaryUseCase>();

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
