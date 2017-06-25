namespace HistSim
{
    public interface IStockData
    {
        int QuotesPerStock { get; }
        float[] RawData { get; }
        int StocksCount { get; }
        float[] MarketValues { get; }
    }
}