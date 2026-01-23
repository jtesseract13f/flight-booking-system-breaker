using GatewayService.DTO.TicketServiceDtos;

namespace GatewayService.DTO.BonusServiceDtos;

public record TicketPurchase(CreatedTicket Ticket, int Price, bool UseBalance);