namespace GatewayService.DTO.BonusServiceDtos;

public record PurchaseInfo(
    int PaidByBonuses,
    int PaidByMoney,
    int Price);