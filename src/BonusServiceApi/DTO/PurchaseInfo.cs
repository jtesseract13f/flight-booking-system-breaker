namespace BonusServiceApi.DTO;

public record PurchaseInfo(
    int PaidByBonuses,
    int PaidByMoney,
    int Price);