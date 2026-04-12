using Dapper.Contrib.Extensions;

namespace TradeHub.DAL.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string Address { get; set; }
        public string PasswordHash { get; set; }
    }
}
