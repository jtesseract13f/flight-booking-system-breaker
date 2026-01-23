namespace TicketServiceApi.DTO;


public record TicketDto(
    Guid TicketUid,
    string Username,
    string FlightNumber,
    int Price,
    string Status);//??