using Polly;

namespace GatewayService.Infrastructure;

public class CircuitBreaker :  DelegatingHandler
{
    private readonly ResiliencePipeline _circuitBreakerPolicy = new ResiliencePipelineBuilder()
        .AddRetry(new() { MaxRetryAttempts = 3 })
        .AddCircuitBreaker(new()
        {
            FailureRatio = 1,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(15)
        })
        .Build();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _circuitBreakerPolicy.ExecuteAsync(async token =>
            await base.SendAsync(request, token), cancellationToken);
    }
}