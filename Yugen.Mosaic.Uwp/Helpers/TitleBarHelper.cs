using Windows.ApplicationModel.Core;

namespace Yugen.Mosaic.Uwp.Views
{
    public static class TitleBarHelper
    {
        public static void ExtendToTitleBar()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }
    }
}