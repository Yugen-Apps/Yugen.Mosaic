using Yugen.Toolkit.Standard.Mvvm;
using Yugen.Toolkit.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class WhatsNewViewModel : ViewModelBase
    {
        public string Title => ResourceHelper.GetText("WhatsNewTitle");
        public string Body => ResourceHelper.GetText("WhatsNewBody");
    }
}