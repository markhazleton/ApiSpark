namespace ApiSpark.Api.Features.Health;

public record HealthResponse(string Status, string Service, string Version);

public record DeepHealthResponse(string Status, Dictionary<string, string> Checks);
