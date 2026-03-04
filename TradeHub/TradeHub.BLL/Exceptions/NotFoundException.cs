namespace TradeHub.BLL.Exceptions
{
    public class NotFoundException : BusinessException
    {
        /// <summary>
        /// "Không tìm thấy {item} có {field} = {value}"
        /// </summary>
        public NotFoundException(string item, string field, object value)
            : base($"Không tìm thấy {item} có {field} = {value}")
        {
        }
    }
}
