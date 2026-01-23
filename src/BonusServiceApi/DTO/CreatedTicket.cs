namespace BonusServiceApi.DTO;

public record CreatedTicket(
    Guid TicketUid,
    string Username,
    string FlightNumber,
    int Price,
    string Status);
