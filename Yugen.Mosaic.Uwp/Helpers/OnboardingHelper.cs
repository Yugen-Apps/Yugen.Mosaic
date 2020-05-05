using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Helpers
{

    public static class OnboardingHelper
    {
        private const string SettingsKey = "OnboardingIsEnabled";

        public static bool IsDisabled
        {
            get => SettingsHelper.Read<bool>(SettingsKey);
            set => SettingsHelper.Write<bool>(SettingsKey, value);
        }

        private static int _step;
        private static OnboardingElement[] _onboardingElements;

        public static void Init(FrameworkElement[] frameworkElements) => _onboardingElements = new OnboardingElement[]
            {
                new OnboardingElement(
                    frameworkElements[0],
                    OnboardingStage.MasterImage),
                new OnboardingElement(
                    frameworkElements[1],
                    OnboardingStage.AddTiles),
                new OnboardingElement(
                    frameworkElements[2],
                    OnboardingStage.TileProperties),
                new OnboardingElement(
                    frameworkElements[3],
                    OnboardingStage.MosaicType),
                new OnboardingElement(
                    frameworkElements[4],
                    OnboardingStage.OutputProperties),
                new OnboardingElement(
                    frameworkElements[5],
                    OnboardingStage.Generate),
                new OnboardingElement(
                    frameworkElements[6],
                    OnboardingStage.Save),
            };

        public static OnboardingElement ShowTeachingTip()
        {
            OnboardingElement onboardingElement = null;

            if (_step < 0 || IsDisabled)
            {
                return onboardingElement;
            }

            if (_step < _onboardingElements.Length)
            {
                onboardingElement = _onboardingElements[_step];
                _step++;
            }
            else
            {
                IsDisabled = true;
            }

            return onboardingElement;
        }
    }
}