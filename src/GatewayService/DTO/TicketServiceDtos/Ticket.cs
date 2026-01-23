namespace GatewayService.DTO.TicketServiceDtos;

public record Ticket(
    Guid TicketUid,
    string Username,
    string FlightNumber,
    int Price,
    string Status);//??


/*
 *     [Key]
       public int Id { get; set; }
       public Guid TicketUid { get; set; }
       [MaxLength(80)]
       public required string Username { get; set; }
       [MaxLength(20)]
       public required string FlightNumber { get; set; }
       public int Price { get;set; }
       public TicketStatusEnum Status { get;set; }
   }
 */