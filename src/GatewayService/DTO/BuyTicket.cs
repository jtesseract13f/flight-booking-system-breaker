namespace GatewayService.DTO;

public record BuyTicket(
    string FlightNumber,
    int Price,
    bool PaidFromBalance);


/*
 * {
       "flightNumber": "{{flightNumber}}",
       "price": 1500,
       "paidFromBalance": false
   }
 */