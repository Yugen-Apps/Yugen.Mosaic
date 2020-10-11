using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Controls;

namespace Yugen.Mosaic.Uwp.Services
{
    public class WhatsNewDisplayService : IWhatsNewDisplayService
    {
        private bool isShown = false;

        public async Task ShowIfAppropriateAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                if (SystemInformation.IsAppUpdated && !isShown)
                {
                    isShown = true;
                    var dialog = new WhatsNewDialog();
                    await dialog.ShowAsync();
                }
            });
        }
    }
}