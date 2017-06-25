using System;
using System.IO;
using OpenCL.Net;
using OpenCL.Net.Extensions;

using ClEnvironment = OpenCL.Net.Environment;

namespace HistSim
{
    public sealed class GpuSimiulation : IDisposable

    {
        private readonly IStockData _stockData;
        private readonly int _holding;
        private string _programSource;
        private ClEnvironment _env;

        private float[] _h_output;

        private readonly IMem<float> _d_stocksAndPrices;
        private readonly IMem<float> _d_portfolioStockMv;
        private readonly IMem<float> _d_output;

        private readonly float _ann;

        public GpuSimiulation(IStockData stockData, int holding = 10, float ann = 5.0990f)
        {
            _stockData = stockData;
            _holding = holding;
            _ann = ann;
            _programSource = File.ReadAllText("Kernels.cl");

            _env = "*AMD*".CreateCLEnvironment(DeviceType.Gpu, CommandQueueProperties.ProfilingEnable);

            _h_output = new float[(stockData.QuotesPerStock - holding)];
            _d_stocksAndPrices = Cl.CreateBuffer<float>(_env.Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr , _stockData.RawData, out ErrorCode err);
            _d_portfolioStockMv = Cl.CreateBuffer<float>(_env.Context, MemFlags.ReadOnly | MemFlags.CopyHostPtr , _stockData.MarketValues, out err);
            _d_output = Cl.CreateBuffer<float>(_env.Context, MemFlags.ReadWrite, stockData.StocksCount * (stockData.QuotesPerStock - holding), out err);
        }

        public float CalculateVar(out double totalMs, float cf = .9f, int iterations = 10)
        {
            using (var annualizedReturnsKernel = _env.Context.CompileKernelFromSource(_programSource, "annualizedReturns"))
            using (var aggregateReturnsKernel = _env.Context.CompileKernelFromSource(_programSource, "aggregateReturnsKernel"))
            {
                Event clStart, clEnd;
                var queue = _env.CommandQueues[0];

                var xSize = (_stockData.QuotesPerStock - _holding);
                var ySize = _stockData.StocksCount;

                var err = Cl.SetKernelArg(annualizedReturnsKernel, 0, _d_output);
                err = Cl.SetKernelArg(annualizedReturnsKernel, 1, _d_stocksAndPrices);
                err = Cl.SetKernelArg(annualizedReturnsKernel, 2, _d_portfolioStockMv);
                err = Cl.SetKernelArg(annualizedReturnsKernel, 3, _ann);
                err = Cl.SetKernelArg(annualizedReturnsKernel, 4, _holding);

                err = Cl.SetKernelArg(aggregateReturnsKernel, 0, _d_output);
                err = Cl.SetKernelArg(aggregateReturnsKernel, 1, ySize);

                Cl.EnqueueMarker(queue, out clStart);
                Cl.Finish(queue);

                for (int i = 0; i < iterations; i++)
                {
                    err = Cl.EnqueueNDRangeKernel(queue, annualizedReturnsKernel, 2, null,
                        new[] { (IntPtr)xSize, (IntPtr)ySize },
                        new[] { (IntPtr)256, (IntPtr)1 },
                        0, null, out Event notInterestedInThisEvent1);

                    err = Cl.EnqueueNDRangeKernel(queue, aggregateReturnsKernel, 2, null,
                        new[] { (IntPtr)xSize, (IntPtr)1 },
                        new[] { (IntPtr)256, (IntPtr)1 },
                        0, null, out notInterestedInThisEvent1);
                }

                Cl.EnqueueMarker(queue, out clEnd);
                Cl.Finish(queue);

                var startInfo = Cl.GetEventProfilingInfo(clStart, ProfilingInfo.Start, out err);
                var endInfo = Cl.GetEventProfilingInfo(clEnd, ProfilingInfo.End, out err);

                Cl.EnqueueReadBuffer<float>(queue, _d_output, Bool.True, _h_output, 0, null, out Event notInterestedInThisEvent2);

                totalMs = (endInfo.CastTo<ulong>() - startInfo.CastTo<ulong>()) * 10e-6;

                clStart.Dispose();
                clEnd.Dispose();
                annualizedReturnsKernel.Dispose();

                return _h_output[0] + _h_output[xSize - 1];
            }
        }

        public void Dispose()
        {
            _d_stocksAndPrices.Dispose();
            _d_output.Dispose();
            _env.Dispose();
        }
    }
}
