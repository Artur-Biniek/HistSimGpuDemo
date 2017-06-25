using System;

namespace HistSim
{
    public class RandomStockData : IStockData
    {
        public int StocksCount { get; private set; }
        public int QuotesPerStock { get; private set; }

        public float[] RawData { get; private set; }

        public float[] MarketValues { get; private set; }

        public RandomStockData(int stocksCount, int quotesPerStock, float minPrice = 10.00f, float maxPrice = 100.00f)
        {
            StocksCount = stocksCount;
            QuotesPerStock = quotesPerStock;

            var limit = stocksCount * quotesPerStock;
            var range = maxPrice - minPrice;
            var rnd = new Random();

            RawData = new float[limit];

            for (int i = 0; i < limit; i++) RawData[i] = (float)rnd.NextDouble() * range + minPrice;

            MarketValues = new float[StocksCount];
            for (int i = 0; i < stocksCount; i++) MarketValues[i] = (float)rnd.NextDouble() * range + minPrice;
        }
    }
}
