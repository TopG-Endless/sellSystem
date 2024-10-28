using System.ComponentModel.DataAnnotations;

namespace SellSystem.DTOs
{
    public class UserLoginDto
    {
        [Key]
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}