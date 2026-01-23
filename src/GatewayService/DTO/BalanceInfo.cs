namespace GatewayService.DTO;

public record BalanceInfo(
  int Balance,
  string Status,
  IEnumerable<HistoryRecord>? History);

public record HistoryRecord(
  DateTime Date,
  Guid TicketUid,
  int BalanceDiff,
  string OperationType
  );
/*
 *
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