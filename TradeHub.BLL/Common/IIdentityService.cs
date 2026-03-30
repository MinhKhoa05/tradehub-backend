namespace TradeHub.BLL.Common
{
    public interface IIdentityService
    {
        int? UserId { get; }
        //bool IsAuthenticated { get; }
        // Sau này muốn lấy Email hay Role thì cứ thêm vào đây
    }
}