namespace BonusServiceApi.DAL;

public enum PrivilegeStatus
{
    BRONZE,
    SILVER,
    GOLD
}

/*
 * status   VARCHAR(80) NOT NULL DEFAULT 'BRONZE'
   CHECK (status IN ('BRONZE', 'SILVER', 'GOLD')),
 */