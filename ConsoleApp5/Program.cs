using System;

namespace HistSim
{
    class Program
    {
        static void Main(string[] args)
        {
            var rndS = new RandomStockData(2048, 256 * 10 + 10);

            var iters = 5;

            var cpu = new CpuSimulation(rndS);
            var gpu = new GpuSimiulation(rndS);

            var res1 = cpu.CalculateVar(out double totalMsCpu, iterations: iters);
            Console.WriteLine("Cpu 1 thread total (ms): {0:0.00} with {1}.", totalMsCpu, res1);

            var res2 = gpu.CalculateVar(out double totalMsGpu, iterations: iters);
            Console.WriteLine("Gpu total (ms): {0:0.00} with {1}", totalMsGpu, res2);

            Console.WriteLine("Diff: {0}", res2 - res1);
            Console.WriteLine("Gain: {0:0.00}x",  totalMsCpu / totalMsGpu);

            var g = rndS;

            gpu.Dispose();
        }
    }
}
