using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameTopUp.DAL.Entities
{
    [Table("game_accounts")]
    public class GameAccount
    {
        [Key]
        public long Id { get; set; } // D�ng long cho d?ng b? v?i c�c b?ng kh�c

        public long UserId { get; set; } // Ch? s? h?u s? d?a ch? n�y

        public long GameId { get; set; } // "freefire", "pubg"...

        public string Name { get; set; } = null!; // T�n g?i nh? (v� d?: "Acc ch�nh c?a em")

        public string AccountIdentifier { get; set; } = null!; // ID TRONG GAME (V� d?: 88776655)

        public string? Server { get; set; } // Server (n?u c�)

        public string? Description { get; set; } // Ghi ch� th�m

        public bool IsDefault { get; set; } = false; // Uu ti�n ch?n nhanh

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
