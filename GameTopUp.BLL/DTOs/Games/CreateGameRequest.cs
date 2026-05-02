using System.ComponentModel.DataAnnotations;

namespace GameTopUp.BLL.DTOs.Games
{
    public class CreateGameRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; } = null!;
        
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
