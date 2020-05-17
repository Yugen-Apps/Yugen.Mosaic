using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Controls;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Standard.Commands;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private readonly MosaicService _mosaicService = new MosaicService();
        private bool _isAddMasterUIVisible = true;
        private bool _isAlignmentGridVisibile = true;
        private bool _isLoading;
        private bool _isTeachingTipOpen;
        private BitmapImage _masterBmpSource = new BitmapImage();
        private BitmapImage _outputBmpSource = new BitmapImage();
        private int _outputHeight = 1000;
        private Image<Rgba32> _outputImage;
        private int _outputWidth = 1000;
        private MosaicType _selectedMosaicType;
        private string _teachingTipSubTitle;
        private FrameworkElement _teachingTipTarget;
        private string _teachingTipTitle;
        private ObservableCollection<TileBmp> _tileBmpCollection = new ObservableCollection<TileBmp>();
        private int _tileHeight = 25;
        private int _tileWidth = 25;
        private ICommand _pointerEnteredCommand;
        private ICommand _pointerExitedCommand;
        private ICommand _addMasterImmageCommand;
        private ICommand _addTilesCommand;
        private ICommand _clickTileCommand;
        private ICommand _generateCommand;
        private ICommand _saveCommand;
        private ICommand _resetCommand;
        private ICommand _helpCommand;
        private ICommand _settingsCommand;

        public MainViewModel()
        {
            SelectedMosaicType = MosaicTypeList[0];
        }

        public bool IsAddMasterUIVisible
        {
            get => _isAddMasterUIVisible;
            set => Set(ref _isAddMasterUIVisible, value);
        }

        public bool IsAlignmentGridVisibile
        {
            get => _isAlignmentGridVisibile;
            set => Set(ref _isAlignmentGridVisibile, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool IsTeachingTipOpen
        {
            get => _isTeachingTipOpen;
            set => Set(ref _isTeachingTipOpen, value);
        }

        public BitmapImage MasterBpmSource
        {
            get => _masterBmpSource;
            set => Set(ref _masterBmpSource, value);
        }

        public List<MosaicType> MosaicTypeList { get; set; } = new List<MosaicType>
        {
           new MosaicType(MosaicTypeEnum.Classic),
           new MosaicType(MosaicTypeEnum.Random),
           new MosaicType(MosaicTypeEnum.AdjustHue),
           new MosaicType(MosaicTypeEnum.PlainColor)
        };

        public BitmapImage OutputBmpSource
        {
            get => _outputBmpSource;
            set => Set(ref _outputBmpSource, value);
        }

        public int OutputHeight
        {
            get => _outputHeight;
            set => Set(ref _outputHeight, value);
        }

        public int OutputWidth
        {
            get => _outputWidth;
            set => Set(ref _outputWidth, value);
        }

        public MosaicType SelectedMosaicType
        {
            get => _selectedMosaicType;
            set => Set(ref _selectedMosaicType, value);
        }

        public string TeachingTipSubTitle
        {
            get => _teachingTipSubTitle;
            set => Set(ref _teachingTipSubTitle, value);
        }

        public FrameworkElement TeachingTipTarget
        {
            get => _teachingTipTarget;
            set => Set(ref _teachingTipTarget, value);
        }

        public string TeachingTipTitle
        {
            get => _teachingTipTitle;
            set => Set(ref _teachingTipTitle, value);
        }

        public ObservableCollection<TileBmp> TileBmpCollection
        {
            get => _tileBmpCollection;
            set => Set(ref _tileBmpCollection, value);
        }

        public int TileHeight
        {
            get => _tileHeight;
            set => Set(ref _tileHeight, value);
        }

        public int TileWidth
        {
            get => _tileWidth;
            set => Set(ref _tileWidth, value);
        }

        private Size OutputSize => new Size(_outputWidth, _outputHeight);
        private Size TileSize => new Size(_tileWidth, _tileHeight);

        public ICommand PointerEnteredCommand => _pointerEnteredCommand ?? (_pointerEnteredCommand = new RelayCommand(PointerEnteredCommandBehavior));
        public ICommand PointerExitedCommand => _pointerExitedCommand ?? (_pointerExitedCommand = new RelayCommand(PointerExitedCommandBehavior));
        public ICommand AddMasterImmageCommand => _addMasterImmageCommand ?? (_addMasterImmageCommand = new AsyncRelayCommand(AddMasterImmageCommandBehavior));
        public ICommand AddTilesCommand => _addTilesCommand ?? (_addTilesCommand = new AsyncRelayCommand(AddTilesCommandBehavior));
        public ICommand ClickTileCommand => _clickTileCommand ?? (_clickTileCommand = new AsyncRelayCommand<TileBmp>(ClickTileCommandBehavior));
        public ICommand GenerateCommand => _generateCommand ?? (_generateCommand = new AsyncRelayCommand(GenerateCommandBehavior));
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new AsyncRelayCommand(SaveCommandBehavior));
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(ResetCommandBehavior));
        public ICommand HelpCommand => _helpCommand ?? (_helpCommand = new RelayCommand(HelpCommandBehavior));
        public ICommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new AsyncRelayCommand(SettingsCommandBehavior));


        public void PointerEnteredCommandBehavior() => IsAddMasterUIVisible = true;

        public void PointerExitedCommandBehavior() => UpdateIsAddMasterUIVisible();

        private async Task AddMasterImmageCommandBehavior()
        {
            IsLoading = true;

            StorageFile masterFile = await FilePickerHelper.OpenFile(
                new List<string> { ".jpg", ".png" },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (masterFile != null)
            {
                using (IRandomAccessStreamWithContentType inputStream = await masterFile.OpenReadAsync())
                using (Stream stream = inputStream.AsStreamForRead())
                {
                    Image<Rgba32> image = _mosaicService.AddMasterImage(stream);

                    using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 400))
                    {
                        InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);
                        await MasterBpmSource.SetSourceAsync(outputStream);
                    }

                    Tuple<int, int> newSize = RatioHelper.Convert(image.Width, image.Height, OutputSize.Width, OutputSize.Height);
                    OutputWidth = newSize.Item1;
                    OutputHeight = newSize.Item2;
                }
            }

            UpdateIsAddMasterUIVisible();

            IsLoading = false;
        }

        private async Task AddTilesCommandBehavior()
        {
            IReadOnlyList<StorageFile> files = await FilePickerHelper.OpenFiles(
                new List<string> { ".jpg", ".png" },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            if (files == null)
            {
                return;
            }

            //button.IsEnabled = false;
            IsLoading = true;

            await Task.Run(() =>
                Parallel.ForEach(files, async file =>
                {
                    using (IRandomAccessStreamWithContentType inputStream = await file.OpenReadAsync())
                    using (Stream stream = inputStream.AsStreamForRead())
                    {
                        Image<Rgba32> image = _mosaicService.AddTileImage(file.DisplayName, stream);

                        using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 200))
                        {
                            InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);

                            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                            {
                                var bmp = new BitmapImage();
                                await bmp.SetSourceAsync(outputStream);

                                TileBmpCollection.Add(new TileBmp(file.DisplayName, bmp));
                            });
                        }
                    }
                })
            );

            IsLoading = false;
            //button.IsEnabled = true;
        }

        private async Task ClickTileCommandBehavior(TileBmp item)
        {
            await MessageDialogHelper.Confirm("Do you want to Remove this picture?",
                "",
                new UICommand("Yes",
                    action =>
                    {
                        TileBmpCollection.Remove(item);
                        _mosaicService.RemoveTileImage(item.Name);
                    }),
                new UICommand("No"));
        }

        private async Task GenerateCommandBehavior()
        {
            //button.IsEnabled = false;
            IsLoading = true;

            await Task.Run(() =>
                _outputImage = _mosaicService.GenerateMosaic(OutputSize, TileSize, SelectedMosaicType.MosaicTypeEnum));

            if (_outputImage != null)
            {
                InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(_outputImage);
                await OutputBmpSource.SetSourceAsync(outputStream);
            }

            IsLoading = false;
            //button.IsEnabled = true;
        }

        private async Task SaveCommandBehavior()
        {
            //button.IsEnabled = false;
            IsLoading = true;

            var fileTypes = new Dictionary<string, List<string>>()
            {
                {FileFormat.Png.ToString(), new List<string>() {FileFormat.Png.GetStringRepresentation()}},
                {FileFormat.Jpg.ToString(), new List<string>() {FileFormat.Jpg.GetStringRepresentation()}}
            };

            StorageFile file = await FilePickerHelper.SaveFile("Mosaic", fileTypes,
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (file == null || _outputImage == null)
            {
                return;
            }

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                switch (file.FileType)
                {
                    case ".png":
                        _outputImage.SaveAsPng(stream.AsStreamForWrite());
                        break;

                    default:
                        _outputImage.SaveAsJpeg(stream.AsStreamForWrite());
                        break;
                }
            }

            IsLoading = false;
            //button.IsEnabled = true;
        }

        private void ResetCommandBehavior()
        {
            _mosaicService.Reset();

            MasterBpmSource = new BitmapImage();
            TileBmpCollection = new ObservableCollection<TileBmp>();

            GC.Collect();

            UpdateIsAddMasterUIVisible();
        }

        private void HelpCommandBehavior()
        {
            OnboardingHelper.IsDisabled = false;
            ShowTeachingTip();
        }

        private async Task SettingsCommandBehavior()
        {
            var settingsDialog = new SettingsDialog();
            await settingsDialog.ShowAsync();
        }


        public void ShowTeachingTip()
        {
            OnboardingElement onboardingElement = OnboardingHelper.ShowTeachingTip();
            if (onboardingElement == null)
            {
                return;
            }

            TeachingTipTitle = onboardingElement.Title;
            TeachingTipSubTitle = onboardingElement.Subtitle;
            TeachingTipTarget = onboardingElement.Target;
            IsTeachingTipOpen = true;
        }

        public void TeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            OnboardingHelper.IsDisabled = true;
            IsTeachingTipOpen = false;
        }

        public void TeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args) => ShowTeachingTip();

        public void TeachingTip_Closing(TeachingTip sender, TeachingTipClosingEventArgs args)
        {
            TeachingTipTitle = "";
            TeachingTipSubTitle = "";
            TeachingTipTarget = null;
            IsTeachingTipOpen = false;
        }


        private void UpdateIsAddMasterUIVisible() => IsAddMasterUIVisible = (MasterBpmSource.PixelWidth > 0 && MasterBpmSource.PixelHeight > 0) ? false : true;
    }
}