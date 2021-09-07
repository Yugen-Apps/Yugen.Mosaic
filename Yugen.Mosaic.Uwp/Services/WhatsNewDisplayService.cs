using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.System;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Views.Dialogs;

namespace Yugen.Mosaic.Uwp.Services
{
    public class WhatsNewDisplayService : IWhatsNewDisplayService
    {
        private bool isShown = false;

        public async Task ShowIfAppropriateAsync()
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if (SystemInformation.Instance.IsAppUpdated && !isShown)
            {
                isShown = true;
                await dispatcherQueue.EnqueueAsync(async () =>
                {
                    var dialog = new WhatsNewDialog();
                    await dialog.ShowAsync();
                });
            }
        }
    }
}