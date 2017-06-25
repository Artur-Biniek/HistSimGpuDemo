using System;
using System.Diagnostics;

namespace HistSim
{
    public class CpuSimulation
    {
        private readonly IStockData _stockData;

        private float[,] _returns;
        private readonly int _holding;
        private readonly float _ann;

        public CpuSimulation(IStockData stockData, int holding = 10, float ann = 5.0990f)
        {
            _stockData = stockData;
            _holding = holding;
            _ann = ann;

            _returns = new float[stockData.StocksCount, stockData.QuotesPerStock - holding];
        }

        public float CalculateVar(out double totalMs, float cf = .9f, int iterations = 10)
        {
            var sw = new Stopwatch();

            sw.Start();

            var limit = _stockData.QuotesPerStock - _holding;

            for (int i = 0; i < iterations; i++)
            {
                for (int stock = 0; stock < _stockData.StocksCount; stock++)
                {
                    var baseAddress = stock * _stockData.QuotesPerStock;

                    for (int quote = 0; quote < limit; quote++)
                    {
                        var index = baseAddress + quote;

                        var ri = (float)Math.Pow((_stockData.RawData[index + _holding] / _stockData.RawData[index]), _ann);

                        _returns[stock, quote] = (ri + 1) * _stockData.MarketValues[stock];
                    }
                }

                for (int stock = 1; stock < _stockData.StocksCount; stock++)
                {
                    for (int quote = 0; quote < limit; quote++)
                    {
                        _returns[0, quote] += _returns[stock, quote];
                    }
                }
            }

            sw.Stop();

            totalMs = sw.Elapsed.TotalMilliseconds;

            var sum = 0.0;

            sum = _returns[0, 0] + _returns[0, limit - 1];

            return (float)sum;
        }
    }
}
