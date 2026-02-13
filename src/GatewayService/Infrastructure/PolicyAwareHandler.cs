using System.Net;
using System.Reflection;
using GatewayService.Infrastructure.Attributes;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace GatewayService.Infrastructure;

public class PolicyAwareHandler : DelegatingHandler
{
    [Obsolete("Obsolete")]
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Properties.TryGetValue("Refit.RestMethodInfo", out var methodObj) && 
            methodObj is Refit.RestMethodInfo methodInfo)
        {
            var policyAttr = methodInfo.MethodInfo.GetCustomAttribute<NeedToDonePolicyAttribute>();
            if (policyAttr == null) return await base.SendAsync(request, cancellationToken);
            try
            {
                var result = await base.SendAsync(request, cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                await StaticQueue.AddMessage(request);
                throw;
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}