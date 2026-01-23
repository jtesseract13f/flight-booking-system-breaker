using GatewayService.DTO;
using GatewayService.DTO.BonusServiceDtos;
using GatewayService.DTO.TicketServiceDtos;
using Refit;

namespace GatewayService.ApiServices;

public interface IBonusApi
{
    [Get("/privilege")]
    public Task<BalanceInfo> GetBalanceInfo([Header("X-User-Name")] string username);
    [Get("/privilege/purchase")]
    public Task<PurchaseInfo> GetPurchaseInfo([Header("X-User-Name")] string username, int price, bool useBalance);
    [Post("/privilege/purchase")]
    public Task<Privilege> ChangeBalance([Header("X-User-Name")] string username, [Body] TicketPurchase ticketPurchase);
    [Delete("/privilege/purchase")]
    public Task<Guid> RevertPurchase([Header("X-User-Name")] string username, Guid ticketGuid);    //returns cancelled ticket uid
}