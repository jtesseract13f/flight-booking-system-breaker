namespace GatewayService.DTO.TicketServiceDtos;

public record CreatedTicket(
    Guid TicketUid,
    string Username,
    string FlightNumber,
    int Price,
    string Status);
    
    /*
     * CREATE TABLE ticket
       (
           id            SERIAL PRIMARY KEY,
           ticket_uid    uuid UNIQUE NOT NULL,
           username      VARCHAR(80) NOT NULL,
           flight_number VARCHAR(20) NOT NULL,
           price         INT         NOT NULL,
           status        VARCHAR(20) NOT NULL
               CHECK (status IN ('PAID', 'CANCELED'))
       );
     */
     
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