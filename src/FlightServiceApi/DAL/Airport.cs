using System.ComponentModel.DataAnnotations;

namespace DAL;

public class Airport
{
    [Key]
    public int Id { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(255)]
    public string City  { get; set; }
    [MaxLength(255)]
    public string Country { get; set; }
    
}