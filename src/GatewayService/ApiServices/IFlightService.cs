using GatewayService.DTO;
using GatewayService.DTO.FlightApiDtos;
using Refit;

namespace GatewayService.ApiServices;

public interface IFlightApi
{
    [Get("/flights/{flightNumber}")]
    Task<Flight>  GetFlightInfo(string flightNumber);
    
    [Get("/flights")]
    Task<IEnumerable<Flight>?> GetAllFlightInfos([Query] int page, [Query] int size);
}