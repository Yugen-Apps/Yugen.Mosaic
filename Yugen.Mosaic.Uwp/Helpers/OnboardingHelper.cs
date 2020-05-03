using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Extensions;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public enum OnboardingStage
    {
        MasterImage,
        AddTiles,
        TileProperties,
        MosaicType,
        OutputProperties,
        Generate,
        Save
    }

    public class OnboardingElement
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public FrameworkElement Target { get; set; }

        public OnboardingStage Stage { get; set; }

        public OnboardingElement(string title, string subtitle, FrameworkElement target, OnboardingStage stage)
        {
            Title = title;
            Subtitle = subtitle;
            Target = target;
            Stage = stage;
        }
    }

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

        public static void Init(FrameworkElement[] frameworkElements)
        {
            var masterImageDescription = "Choose what image you want to use as the Matrix of the mosaic. " +
                                         "Yugen Mosaic will create a mosaic as close as possible to the main image.";

            var addTilesDescription = "Add a list of Tile Images, Yugen Mosaic will use as tiles to build your mosaic";

            var tilesPropertiesDescription = "After you created the Tile Images List, it is necessary to set all the parameters of the mosaic." +
                                             "Here you can set the size of every single tile";

            var mosaicTypeDescription = "Choose the tpe of mosaic.";

            var outputDescription = "Choose the size of the mosaic.";

            var generateDescription = "Create the mosaic.";

            var saveDescription = "The last step is to save the mosaic.";

            _onboardingElements = new OnboardingElement[]
            {
                new OnboardingElement("Select the Main Image", masterImageDescription, 
                    frameworkElements[0], OnboardingStage.MasterImage),
                new OnboardingElement("Create a Tile Images List", addTilesDescription, 
                    frameworkElements[1], OnboardingStage.AddTiles),
                new OnboardingElement("TileProperties", tilesPropertiesDescription, 
                    frameworkElements[2], OnboardingStage.TileProperties),
                new OnboardingElement("MosaicType", mosaicTypeDescription, 
                    frameworkElements[3], OnboardingStage.MosaicType),
                new OnboardingElement("Set the parameters of the Mosaic", outputDescription, 
                    frameworkElements[4], OnboardingStage.OutputProperties),
                new OnboardingElement("Create the Mosaic", generateDescription,
                    frameworkElements[5], OnboardingStage.Generate),
                new OnboardingElement("Save the Mosaic", saveDescription,
                    frameworkElements[6], OnboardingStage.Save),
            };
        }

        public static OnboardingElement ShowTeachingTip()
        {
            OnboardingElement onboardingElement = null;

            if (_step < 0 || IsDisabled)
                return onboardingElement;

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