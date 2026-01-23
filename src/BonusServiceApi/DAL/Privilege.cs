using System.ComponentModel.DataAnnotations;

namespace BonusServiceApi.DAL;

public class Privilege
{
    [Key]
    public int Id { get; set; }
    [MaxLength(80)]
    public required string Username { get; set; }
    public PrivilegeStatus Status { get; set; } = PrivilegeStatus.BRONZE;
    public uint Balance { get; set; }

    public ICollection<PrivilegeHistory> History { get; set; } = new List<PrivilegeHistory>();
}

/*
 * CREATE TABLE privilege
   (
       id       SERIAL PRIMARY KEY,
       username VARCHAR(80) NOT NULL UNIQUE,
       status   VARCHAR(80) NOT NULL DEFAULT 'BRONZE'
           CHECK (status IN ('BRONZE', 'SILVER', 'GOLD')),
       balance  INT
   );

 */