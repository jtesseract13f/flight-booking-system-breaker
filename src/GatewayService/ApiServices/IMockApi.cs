using GatewayService.DTO.FlightApiDtos;
using GatewayService.Infrastructure;
using GatewayService.Infrastructure.Attributes;
using Refit;

namespace GatewayService.ApiServices;

public interface IMockApi
{
    [NeedToDonePolicy]
    [Get("/flights/{flightNumber}")]
    Task<Flight>  GetFlightInfo(string flightNumber);
}