using System.Collections.Concurrent;
using System.Net;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiSpark.Api.Tests.Infrastructure.Observability;

/// <summary>
/// Verifies FR-009: all 9 required structured log fields are captured
/// by RequestLoggingMiddleware on every request.
/// Fields: Method, RequestPath, StatusCode, DurationMs,
///         CorrelationId, UserId, FeatureName, OperationName, Success
/// </summary>
public class RequestLoggingMiddlewareTests : IClassFixture<LoggingTestFactory>
{
    private readonly LoggingTestFactory _factory;
    private readonly HttpClient _client;

    public RequestLoggingMiddlewareTests(LoggingTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RequestLogging_CapturesMethod()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("HTTP GET"));
    }

    [Fact]
    public async Task RequestLogging_CapturesRequestPath()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("/api/health"));
    }

    [Fact]
    public async Task RequestLogging_CapturesStatusCode()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("responded 200"));
    }

    [Fact]
    public async Task RequestLogging_CapturesDurationMs()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("ms |"));
    }

    [Fact]
    public async Task RequestLogging_CapturesCorrelationId()
    {
        _factory.Capture.Messages.Clear();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        req.Headers.Add("X-Correlation-ID", "test-corr-123");
        await _client.SendAsync(req);
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("CorrelationId=test-corr-123"));
    }

    [Fact]
    public async Task RequestLogging_UserId_IsAnonymousForUnauthenticated()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("UserId=anonymous"));
    }

    [Fact]
    public async Task RequestLogging_CapturesFeatureName()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("Feature="));
    }

    [Fact]
    public async Task RequestLogging_CapturesOperationName()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("Operation="));
    }

    [Fact]
    public async Task RequestLogging_CapturesSuccessTrue_For2xx()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/health");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("Success=True"));
    }

    [Fact]
    public async Task RequestLogging_CapturesSuccessFalse_For4xx()
    {
        _factory.Capture.Messages.Clear();
        await _client.GetAsync("/api/public/content/articles/nonexistent-article");
        Assert.Contains(_factory.Capture.Messages, m => m.Contains("Success=False"));
    }

    [Fact]
    public async Task RequestLogging_ResponseHeader_ContainsCorrelationId()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.True(response.Headers.Contains("X-Correlation-ID"),
            "Response should echo X-Correlation-ID header");
    }
}

/// <summary>
/// Test factory that extends the shared ApiSparkWebApplicationFactory and
/// adds a CaptureLoggerProvider so tests can assert on log message content.
/// Inherits SQLite shared-cache setup and TestAuthHandler from the base class.
/// </summary>
public class LoggingTestFactory : ApiSparkWebApplicationFactory
{
    public CaptureLoggerProvider Capture { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
            services.AddSingleton<ILoggerProvider>(Capture));
    }
}

/// <summary>
/// Thread-safe ILoggerProvider that accumulates formatted log messages.
/// </summary>
public sealed class CaptureLoggerProvider : ILoggerProvider
{
    public ConcurrentBag<string> Messages { get; } = [];

    public ILogger CreateLogger(string categoryName) => new CaptureLogger(Messages);

    public void Dispose() { }
}

internal sealed class CaptureLogger(ConcurrentBag<string> messages) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            messages.Add(formatter(state, exception));
    }
}
