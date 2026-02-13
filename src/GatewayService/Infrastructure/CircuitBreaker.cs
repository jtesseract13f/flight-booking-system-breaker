using Polly;

namespace GatewayService.Infrastructure;

public class CircuitBreaker : DelegatingHandler
{
    private readonly ResiliencePipeline _pipeline;
    public CircuitBreaker()
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new() {  FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(15) })
            .Build();
    }
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _pipeline.ExecuteAsync(async token =>
            await base.SendAsync(request, token), cancellationToken);
    }
}