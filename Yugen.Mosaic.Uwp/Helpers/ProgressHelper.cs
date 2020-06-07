using System;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class ProgressHelper
    {
        private static int _progress;

        public static Progress<int> Progress;

        public static void Init(Progress<int> progress) => Progress = progress;

        public static void ResetProgress() => _progress = 0;

        public static void IncrementProgress(int start, int end, int count)
        {
            IncrementProgress(Progress, start, end, count);
        }

        private static void IncrementProgress(IProgress<int> progress, int start, int end, int count)
        {
            _progress++;
            var currentPercentage = _progress * end / count;
            var totalPercentage = start + currentPercentage;
            progress.Report(totalPercentage);
        }
    }

}