using System.Diagnostics;

namespace Yugen.Mosaic.Uwp.Helpers
{
    /// <summary>
    /// 50x50 200x200
    /// -2 Elapsed: 00:00:09.4523085
    /// -5 Elapsed: 00:00:21.7696783
    /// </summary>
    public class BenchmarkHelper
    {
        private readonly Stopwatch _sw = new Stopwatch();

        public void Start()
        {
            _sw.Restart();
            //_sw.Start();
        }

        public void Stop(string text)
        {
            _sw.Stop();
            Debug.WriteLine($"-{text} Elapsed: {_sw.Elapsed}");
        }
    }
}
