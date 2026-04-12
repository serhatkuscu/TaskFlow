using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace TaskFlow.API.Middleware;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Dictionary<Type, HttpStatusCode> ExceptionStatusMap = new()
    {
        { typeof(KeyNotFoundException),       HttpStatusCode.NotFound },
        { typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized },
        { typeof(ArgumentException),          HttpStatusCode.BadRequest },
        { typeof(ArgumentNullException),      HttpStatusCode.BadRequest },
        { typeof(InvalidOperationException),  HttpStatusCode.BadRequest },
        { typeof(NotSupportedException),      HttpStatusCode.BadRequest },
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmedik hata oluştu. Path: {Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response zaten başladığı için hata yanıtı yazılamıyor.");
                return;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = ExceptionStatusMap.TryGetValue(exception.GetType(), out var mappedCode)
            ? mappedCode
            : HttpStatusCode.InternalServerError;

        var problem = new ProblemDetails
        {
            Status   = (int)statusCode,
            Title    = GetTitle(statusCode),
            Detail   = _env.IsDevelopment() ? exception.Message : GetSafeMessage(statusCode),
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound            => "Kaynak bulunamadı.",
        HttpStatusCode.Unauthorized        => "Yetkisiz erişim.",
        HttpStatusCode.BadRequest          => "Geçersiz istek.",
        HttpStatusCode.InternalServerError => "Sunucu hatası.",
        _                                  => "Bir hata oluştu."
    };

    private static string GetSafeMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound            => "İstenen kaynak bulunamadı.",
        HttpStatusCode.Unauthorized        => "Bu işlem için yetkiniz bulunmuyor.",
        HttpStatusCode.BadRequest          => "Gönderilen istek geçersiz.",
        HttpStatusCode.InternalServerError => "Beklenmedik bir hata oluştu.",
        _                                  => "Bir hata oluştu."
    };
}
