using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class ClientDTO
{
    public int Id { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email{ get; set; }
    
    [Required]
    [Phone]
    public string Telephone{ get; set; }
    
    [Required]
    [RegularExpression(@"^\d{11}$")]
    public string Pesel { get; set; }
    
    public List<ClientTripDTO> Trips { get; set; } = [];
}