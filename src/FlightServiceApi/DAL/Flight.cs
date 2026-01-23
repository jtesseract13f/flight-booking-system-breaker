using System.ComponentModel.DataAnnotations;

namespace DAL;

public class Flight
{
    [Key]
    public int Id { get; set; }
    [MaxLength(20)]
    public string FlightNumber { get; set; }
    public DateTime Datetime {get; set;}
    public int FromAirportId { get; set; }
    public int ToAirportId { get; set; }
    public int Price { get; set; }
    
    public Airport FromAirport { get; set; }
    public Airport ToAirport { get; set; }
}