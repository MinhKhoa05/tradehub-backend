using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("game_accounts")]
    public class GameAccount
    {
        [Key]
        public long Id { get; set; } // Dùng long cho d?ng b? v?i các b?ng khác

        public long UserId { get; set; } // Ch? s? h?u s? d?a ch? này

        public long GameId { get; set; } // "freefire", "pubg"...

        public string Name { get; set; } = null!; // Tên g?i nh? (ví d?: "Acc chính c?a em")

        public string AccountIdentifier { get; set; } = null!; // ID TRONG GAME (Ví d?: 88776655)

        public string? Server { get; set; } // Server (n?u có)

        public string? Description { get; set; } // Ghi chú thêm

        public bool IsDefault { get; set; } = false; // Uu tiên ch?n nhanh

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
