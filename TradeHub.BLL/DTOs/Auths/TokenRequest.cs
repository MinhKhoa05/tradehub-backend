namespace TradeHub.BLL.DTOs.Auths
{
    public class TokenRequest
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
