namespace TechMove.GLMS.Interfaces
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertUsdToZarAsync(decimal amountUsd);
    }
}
