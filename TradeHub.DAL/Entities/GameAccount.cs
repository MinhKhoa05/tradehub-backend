using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeHub.DAL.Entities
{
    [Table("game_accounts")]
    public class GameAccount
    {
        [Key]
        public long Id { get; set; } // Dùng long cho đồng bộ với các bảng khác

        public long UserId { get; set; } // Chủ sở hữu sổ địa chỉ này

        public long GameId { get; set; } // "freefire", "pubg"...

        public string Name { get; set; } = null!; // Tên gợi nhớ (ví dụ: "Acc chính của em")

        public string AccountIdentifier { get; set; } = null!; // ID TRONG GAME (Ví dụ: 88776655)

        public string? Server { get; set; } // Server (nếu có)

        public string? Description { get; set; } // Ghi chú thêm

        public bool IsDefault { get; set; } = false; // Ưu tiên chọn nhanh

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}