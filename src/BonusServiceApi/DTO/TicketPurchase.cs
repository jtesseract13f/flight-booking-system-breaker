namespace BonusServiceApi.DTO;

public record TicketPurchase(CreatedTicket Ticket, int Price, bool UseBalance);