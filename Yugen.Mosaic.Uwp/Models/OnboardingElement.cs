using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.Models
{
    public class OnboardingElement
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public FrameworkElement Target { get; set; }

        public OnboardingStage Stage { get; set; }

        public OnboardingElement(FrameworkElement target, OnboardingStage stage)
        {
            Title = ResourceHelper.GetText($"OnboardingStage{stage}Title");
            Subtitle = ResourceHelper.GetText($"OnboardingStage{stage}Description");
            Target = target;
            Stage = stage;
        }
    }
}