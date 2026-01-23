namespace TicketServiceApi;

public record TicketInfo(
    Guid TicketUid,
    string FlightNumber,
    string Username,
    int Price,
    string Status
    );
    
    /*
     * {
        "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
        "flightNumber": "AFL031",
        "fromAirport": "Санкт-Петербург Пулково",
        "toAirport": "Москва Шереметьево",
        "date": "2021-10-08 20:00",
        "price": 1500,
        "status": "PAID"
       }
     */