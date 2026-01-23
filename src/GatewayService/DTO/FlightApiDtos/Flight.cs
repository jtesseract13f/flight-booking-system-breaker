namespace GatewayService.DTO.FlightApiDtos;

public record Flight(
    string FlightNumber,
    DateTime? Date,
    string? FromAirport,
    string? ToAirport,
    int Price
    );