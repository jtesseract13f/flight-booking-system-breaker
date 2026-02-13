using GatewayService.DTO;
using GatewayService.DTO.FlightApiDtos;
using GatewayService.DTO.TicketServiceDtos;
using GatewayService.Infrastructure.Attributes;
using Refit;

namespace GatewayService.ApiServices;

public interface ITicketApi
{
    [Post("/api/v1/tickets")]
    public Task<CreatedTicket> CreateTicket([Body] Flight flightInfo, [Query]int price, [Header("X-User-Name")] string username); //TODO: refactoring
    
    [Get("/api/v1/tickets/{ticketUid}")]
    public Task<Ticket?> GetTicket(Guid ticketUid, [Header("X-User-Name")] string username);

    [NeedToDonePolicy]
    [Delete("/api/v1/tickets/{ticketUid}")]
    public Task<Guid> CancelTicket(Guid ticketUid);

    [Get("/api/v1/tickets")]
    public Task<IEnumerable<Ticket>> GetUserTickets([Header("X-User-Name")] string username);
}