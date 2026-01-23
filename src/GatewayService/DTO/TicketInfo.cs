namespace GatewayService.DTO;

public record TicketInfo(
    Guid TicketUid,
    string FlightNumber,
    string? FromAirport,
    string? ToAirport,
    DateTime? Date,
    int Price,
    string Status);
    