namespace TradeHub.BLL.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message = "Không tìm thấy dữ liệu")
            : base(message)
        {
        }
    }
}
