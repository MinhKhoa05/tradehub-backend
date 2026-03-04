using System.ComponentModel.DataAnnotations;

namespace TradeHub.DAL.Entities
{
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }

        [Range(0, int.MaxValue)]
        public int Balance { get; set; }

    }
}
