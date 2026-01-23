namespace GatewayService.DTO;

public record PurchasedTicketInfo(
    Guid TicketUid,
    string FlightNumber,
    string? FromAirport,
    string? ToAirport,
    DateTime? Date,
    int Price,
    int PaidByMoney,
    int PaidByBonuses,
    string Status,
    Privilege Privilege
    );


/*
 *
 * {
       "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
       "flightNumber": "AFL031",
       "fromAirport": "Санкт-Петербург Пулково",
       "toAirport": "Москва Шереметьево",
       "date": "2021-10-08 20:00",
       "price": 1500,
       "paidByMoney": 1500,
       "paidByBonuses": 0,
       "status": "PAID",
       
       
       "privilege": {
           "balance": 1500,
           "status": "GOLD"
       }
   }
 */