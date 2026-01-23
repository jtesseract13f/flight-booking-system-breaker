namespace GatewayService.DTO;

public record FlightInfo(
    string FlightNumber, 
    string FromAirport, 
    string ToAirport,
    DateTime Date,
    int Price);

/*
 *
 * {
       "page": 1,
       "pageSize": 1,
       "totalElements": 1,
       "items": [
           {
               "flightNumber": "AFL031",
               "fromAirport": "Санкт-Петербург Пулково",
               "toAirport": "Москва Шереметьево",
               "date": "2021-10-08 20:00",
               "price": 1500
           }
       ]
   }
 */
 