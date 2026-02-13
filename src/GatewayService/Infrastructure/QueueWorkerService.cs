using Polly.CircuitBreaker;

namespace GatewayService.Infrastructure;

public class QueueWorkerService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<QueueWorkerService> _logger;

    public QueueWorkerService(IHttpClientFactory httpClientFactory, ILogger<QueueWorkerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            StaticQueue.SerializableHttpRequest? msg = null;
            try
            {
                if (StaticQueue.Queue.TryTake(out msg))
                {
                    var request = await msg.ToHttpRequestMessage();
                    var httpClient = _httpClientFactory.CreateClient("configured-inner-client");
                    var response = await httpClient.SendAsync(request, stoppingToken);
                    _logger.LogInformation("Message sent, status code: {StatusCode}", response.StatusCode);
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (BrokenCircuitException) when (msg != null)
            {
                StaticQueue.Queue.Add(msg, stoppingToken);
                _logger.LogWarning("BrokenCircuitException: message returned to queue");
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                if (msg != null)
                {
                     StaticQueue.Queue.Add(msg);
                     _logger.LogWarning("General Exception: message returned to queue");
                }
                _logger.LogError(ex, "QueueWorker Exception");
            }
        }
    }
}