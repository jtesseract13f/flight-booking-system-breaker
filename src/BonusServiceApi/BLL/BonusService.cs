using BonusServiceApi.DAL;
using BonusServiceApi.DTO;
using Microsoft.EntityFrameworkCore;

namespace BonusServiceApi.BLL;

public class BonusService(BonusDbContext context)
{
    public async Task<BalanceWithHistory> GetPrivilege(string username)
    {
        var privilegeEntity = context.Privileges
            .Include(x => x.History)
            .FirstOrDefault(x => x.Username == username);
        if (privilegeEntity == null) return new BalanceWithHistory(0, "BRONZE", new List<History>());

        return new BalanceWithHistory(privilegeEntity.Balance,
            Enum.GetName(typeof(PrivilegeStatus), privilegeEntity.Status) ?? "BRONZE",
            privilegeEntity.History.Select(x => new History(
                x.CreateDate.ToUniversalTime(),
                x.TicketUid,
                x.BalanceDiff,
                Enum.GetName(typeof(OperationType), x.OperationType))).ToList()
        );
    } 
    public async Task<PurchaseInfo> GetPurchaseInfo(string username, int price, bool useBalance)
    {
        if (useBalance)
        {
            var balance = await context.Privileges.FirstOrDefaultAsync(x=> x.Username ==  username);
            if (balance is null) return new PurchaseInfo(0, price, price);
            long usedBonuses = 0;
            if (balance.Balance < price) usedBonuses = balance.Balance;
            else usedBonuses = price;
            return new PurchaseInfo((int)usedBonuses, (int)(price-usedBonuses), price);
        }
        return  new PurchaseInfo(0, price, price);
    }
    
    /*    /*
        * Возврат билета
          Билет помечается статусом CANCELED, 
          в Bonus Service в зависимости 
          от типа операции выполняется возврат бонусов 
          на счёт или списание ранее начисленных. 
          При списании бонусный счет не может стать меньше 0.
          
          DELETE {{baseUrl}}/api/v1/tickets/{{ticketUid}}
          X-User-Name: {{username}}
        * /*/
    public async Task<Guid?> RevertPurchase(string username,  Guid ticketGuid)
    {
        var balance = await context.Privileges.Include(x=>x.History).FirstOrDefaultAsync(x=> x.Username == username);
        if (balance is null) return null;
        
        var purchaseToRevert = balance.History.FirstOrDefault(x => x.TicketUid == ticketGuid);
        if (purchaseToRevert is null) return null;

        var revertPrivileHistory = new PrivilegeHistory()
        {
            TicketUid = purchaseToRevert.TicketUid,
            CreateDate = DateTimeOffset.Now.UtcDateTime,
            Privilege = balance,
            PrivilegeId = balance.Id,
            BalanceDiff = purchaseToRevert.BalanceDiff
        };
        if (purchaseToRevert.OperationType == OperationType.DEBIT_THE_ACCOUNT)
        {
            revertPrivileHistory.OperationType = OperationType.FILL_IN_BALANCE;
            balance.Balance += (uint)revertPrivileHistory.BalanceDiff;
        }
        else
        {
            revertPrivileHistory.OperationType = OperationType.DEBIT_THE_ACCOUNT;
            balance.Balance -= (uint)revertPrivileHistory.BalanceDiff;
        }
        balance.History.Add(revertPrivileHistory);
        await context.SaveChangesAsync();
        return ticketGuid;
    }
    
    /*
     * Если при покупке указан флаг "paidFromBalance": true, то с бонусного счёта списываются максимальное количество баллов в отношении 1 балл – 1 рубль.
       Т.е. если на бонусном счете было 500 бонусов,
       билет стоит 1500 рублей и при покупке был указан флаг "paidFromBalance": true",
       то со счёта спишется 500 бонусов (в ответе будет указано "paidByBonuses": 500),
       а стоимость билета будет 1000 рублей (в ответе будет указано "paidByMoney": 1000).
       В сервисе Bonus Service в таблицу privilegeHistory будет добавлена запись о списании со счёта 500 бонусов.
       Если при покупке был указан флаг "paidFromBalance": false, то в ответе будет "paidByBonuses": 0,
        а на бонусный счет будет начислено бонусов в размере 10% от стоимости заказа.
         Так же в таблицу privilegeHistory будет добавлена запись о зачислении бонусов.
     */
    public async Task<PrivilegeDto> ChangeBalance(string username, TicketPurchase purchase)
    {
        var balance = await context.Privileges.FirstOrDefaultAsync(x=> x.Username ==  username);
        if (balance is null) balance = await CreateUser(username);
        
        var privilegeHistoryEntity = new PrivilegeHistory()
        {
            TicketUid = purchase.Ticket.TicketUid,
            CreateDate = DateTimeOffset.Now.UtcDateTime,
            Privilege = balance,
            PrivilegeId = balance.Id
        };
        if (!purchase.UseBalance || balance.Balance == 0)
        {
            var addBonuses = purchase.Price * 0.1;
            privilegeHistoryEntity.OperationType = OperationType.FILL_IN_BALANCE;
            privilegeHistoryEntity.BalanceDiff = (int)addBonuses;
            balance.Balance += (uint)addBonuses;
        }
        else
        {
            var useBonuses = 0;
            if (balance.Balance < purchase.Price) useBonuses = (int)balance.Balance;
            else useBonuses = purchase.Price;
            privilegeHistoryEntity.BalanceDiff = useBonuses;
            privilegeHistoryEntity.OperationType =  OperationType.DEBIT_THE_ACCOUNT;
            balance.Balance -= (uint)useBonuses;
        }
        if (balance.History is null) balance.History = new List<PrivilegeHistory>();
        balance.History.Add(privilegeHistoryEntity);
        await context.SaveChangesAsync();
        return new PrivilegeDto((int)balance.Balance, Enum.GetName(typeof(PrivilegeStatus), balance.Status) ?? "BRONZE");
    }

    async Task<Privilege> CreateUser(string username)
    {
        var newUser = new Privilege()
        {
            Username = username,
            Balance = 0,
            Status = PrivilegeStatus.BRONZE
        };
        await context.Privileges.AddAsync(newUser);
        await context.SaveChangesAsync();
        var balance = await context.Privileges.FirstOrDefaultAsync(x=> x.Username == username);
        if (balance is null) throw new Exception("Cannot create user");
        return balance;
    }
    
    
}