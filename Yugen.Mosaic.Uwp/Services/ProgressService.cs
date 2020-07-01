using System;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class ProgressService
    {
        private IProgress<int> _progress;
        private int current;

        public static ProgressService Instance { get; } = new ProgressService();

        public void Init(Action<int> progress)
        {
            // The Progress<T> constructor captures our UI context,
            // so the lambda will be run on the UI thread.
            _progress = new Progress<int>(progress);
            current = 0;
        }

        public void Reset()
        {
            current = 0;
        }

        public void IncrementProgress(int total, int startPercentage = 0, int maxPercentage = 100)
        {
            var currentPercentage = current * (maxPercentage - startPercentage ) / total;
            ++current;
            _progress.Report(startPercentage + currentPercentage);
        }
    }
}