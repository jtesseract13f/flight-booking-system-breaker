namespace TicketServiceApi;

public record FlightDto(
    string FlightNumber,
    DateTime Date,
    string FromAirport,
    string ToAirport,
    int Price
);