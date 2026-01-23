using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonusServiceApi.DAL;

public class PrivilegeHistory
{
    [Key]
    public int Id { get; set; }
    public Guid TicketUid { get; set; }
    public DateTime CreateDate { get; set; }
    public int BalanceDiff { get; set; }
    public OperationType OperationType { get; set; }
    
    public int PrivilegeId { get; set; }
    public Privilege Privilege { get; set; }
}

/*
 *    
   CREATE TABLE privilege_history
   (
       id             SERIAL PRIMARY KEY,
       privilege_id   INT REFERENCES privilege (id),
       ticket_uid     uuid        NOT NULL,
       datetime       TIMESTAMP   NOT NULL,
       balance_diff   INT         NOT NULL,
       operation_type VARCHAR(20) NOT NULL
           CHECK (operation_type IN ('FILL_IN_BALANCE', 'DEBIT_THE_ACCOUNT'))
   );
 */