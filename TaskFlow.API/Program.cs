using System.Text;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskFlow.API.Middleware;
using TaskFlow.API.Services;
using TaskFlow.Application.Features.Auth;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Application.Features.Tasks.Create;
using TaskFlow.Application.Features.Tasks.Get;
using TaskFlow.Application.Features.Tasks.GetById;
using TaskFlow.Application.Features.Tasks.Update;
using TaskFlow.Application.Validators;
using TaskFlow.Infrastructure.DependencyInjection;

// Bootstrap logger: catches fatal errors before the host is fully built
// (e.g. missing config, DI failures). Replaced by the full Serilog logger below.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("TaskFlow API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Replace the default .NET logging pipeline with Serilog.
    // Configuration is read from appsettings.json "Serilog" section.
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // IHttpContextAccessor is not registered by default in ASP.NET Core.
    // CurrentUserService depends on it to read the active request's claims.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name        = "Authorization",
            Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme      = "Bearer",
            BearerFormat = "JWT",
            In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Token'ı girin. Örnek: eyJhbGci..."
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<CreateTaskHandler>();
    builder.Services.AddScoped<GetAllTasksHandler>();
    builder.Services.AddScoped<GetTaskByIdHandler>();
    builder.Services.AddScoped<UpdateTaskHandler>();
    builder.Services.AddScoped<RegisterHandler>();
    builder.Services.AddScoped<LoginHandler>();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var key        = jwtSection["Key"]!;
    var issuer     = jwtSection["Issuer"]!;
    var audience   = jwtSection["Audience"]!;

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidIssuer              = issuer,

                ValidateAudience         = true,
                ValidAudience            = audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    // Serilog request logging: logs each HTTP request with method, path,
    // status code, and elapsed time. Placed after ExceptionMiddleware so
    // unhandled exceptions are already handled before this logs the outcome.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "TaskFlow API beklenmedik bir hatayla kapandı.");
}
finally
{
    Log.CloseAndFlush();
}
