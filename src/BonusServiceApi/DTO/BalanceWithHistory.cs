namespace BonusServiceApi.DTO;

public record BalanceWithHistory(
  uint Balance,
  string Status,
  List<History> History
  );

/*
 * {
     "balance": 1500,
     "status": "GOLD",
     "history": [
       {
         "date": "2021-10-08T19:59:19Z",
         "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
         "balanceDiff": 1500,
         "operationType": "FILL_IN_BALANCE"
       }
     ]
   }
 */