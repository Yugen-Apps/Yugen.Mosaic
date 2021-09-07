using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Views.Dialogs;
using Yugen.Toolkit.Standard.Core.Models;
using Yugen.Toolkit.Standard.Extensions;
using Yugen.Toolkit.Standard.Helpers;
using Yugen.Toolkit.Standard.Mvvm;
using Yugen.Toolkit.Standard.Services;
using Yugen.Toolkit.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IMosaicService _mosaicService;
        private readonly IProgressService _progressService;

        private bool _isAddMasterUIVisible = true;
        private bool _isAlignmentGridVisibile = true;
        private bool _isIndeterminate;
        private bool _isLoading;
        private bool _isTeachingTipOpen;
        private bool _isButtonEnabled = true;
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
        private int _progress = 0;
        private bool _isSaveAsTextButtonVisible;

        public MainViewModel(IMosaicService mosaicService, IProgressService progressService)
        {
            SelectedMosaicType = MosaicTypeList[0];
            _mosaicService = mosaicService;
            _progressService = progressService;

            PointerEnteredCommand = new RelayCommand(PointerEnteredCommandBehavior);
            PointerExitedCommand = new RelayCommand(PointerExitedCommandBehavior);
            AddMasterImageCommand = new AsyncRelayCommand(AddMasterImageCommandBehavior);
            AddTilesCommand = new AsyncRelayCommand(AddTilesCommandBehavior);
            AddTilesFolderCommand = new AsyncRelayCommand(AddTilesFolderCommandBehavior);
            ClickTileCommand = new AsyncRelayCommand<TileBmp>(ClickTileCommandBehavior);
            GenerateCommand = new AsyncRelayCommand(GenerateCommandBehavior);
            SaveCommand = new AsyncRelayCommand(SaveCommandBehavior);
            SaveAsTextCommand = new AsyncRelayCommand(SaveAsTextCommandBehavior);
            ResetCommand = new RelayCommand(ResetCommandBehavior);
            HelpCommand = new RelayCommand(HelpCommandBehavior);
            WhatsNewCommand = new AsyncRelayCommand(WhatsNewCommandBehavior);
            SettingsCommand = new AsyncRelayCommand(SettingsCommandBehavior);
            TeachingTipActionButtonCommand = new RelayCommand(TeachingTipActionButtonCommandBehavior);
            TeachingTipClosingCommand = new RelayCommand(TeachingTipClosingCommandBehavior);
            TeachingTipClosedCommand = new RelayCommand(TeachingTipClosedCommandBehavior);
        }

        public bool IsAddMasterUIVisible
        {
            get => _isAddMasterUIVisible;
            set => SetProperty(ref _isAddMasterUIVisible, value);
        }

        public bool IsAlignmentGridVisibile
        {
            get => _isAlignmentGridVisibile;
            set => SetProperty(ref _isAlignmentGridVisibile, value);
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetProperty(ref _isIndeterminate, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsTeachingTipOpen
        {
            get => _isTeachingTipOpen;
            set => SetProperty(ref _isTeachingTipOpen, value);
        }

        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set => SetProperty(ref _isButtonEnabled, value);
        }

        public BitmapImage MasterBpmSource
        {
            get => _masterBmpSource;
            set => SetProperty(ref _masterBmpSource, value);
        }

        public List<MosaicType> MosaicTypeList { get; set; } = new List<MosaicType>
        {
           new MosaicType(MosaicTypeEnum.Classic),
           new MosaicType(MosaicTypeEnum.Random),
           new MosaicType(MosaicTypeEnum.AdjustHue),
           new MosaicType(MosaicTypeEnum.PlainColor),
           new MosaicType(MosaicTypeEnum.AsciiArt)
        };

        public BitmapImage OutputBmpSource
        {
            get => _outputBmpSource;
            set => SetProperty(ref _outputBmpSource, value);
        }

        public int OutputHeight
        {
            get => _outputHeight;
            set => SetProperty(ref _outputHeight, value);
        }

        public int OutputWidth
        {
            get => _outputWidth;
            set => SetProperty(ref _outputWidth, value);
        }

        public MosaicType SelectedMosaicType
        {
            get => _selectedMosaicType;
            set
            {
                if (SetProperty(ref _selectedMosaicType, value))
                {
                    IsSaveAsTextButtonVisible = _selectedMosaicType.MosaicTypeEnum == MosaicTypeEnum.AsciiArt;
                }
            }
        }

        public bool IsSaveAsTextButtonVisible
        {
            get => _isSaveAsTextButtonVisible;
            set => SetProperty(ref _isSaveAsTextButtonVisible, value);
        }

        public string TeachingTipSubTitle
        {
            get => _teachingTipSubTitle;
            set => SetProperty(ref _teachingTipSubTitle, value);
        }

        public FrameworkElement TeachingTipTarget
        {
            get => _teachingTipTarget;
            set => SetProperty(ref _teachingTipTarget, value);
        }

        public string TeachingTipTitle
        {
            get => _teachingTipTitle;
            set => SetProperty(ref _teachingTipTitle, value);
        }

        public ObservableCollection<TileBmp> TileBmpCollection
        {
            get => _tileBmpCollection;
            set => SetProperty(ref _tileBmpCollection, value);
        }

        public int TileHeight
        {
            get => _tileHeight;
            set => SetProperty(ref _tileHeight, value);
        }

        public int TileWidth
        {
            get => _tileWidth;
            set => SetProperty(ref _tileWidth, value);
        }

        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public ICommand PointerEnteredCommand { get; }

        public ICommand PointerExitedCommand { get; }

        public ICommand AddMasterImageCommand { get; }

        public ICommand AddTilesCommand { get; }

        public ICommand AddTilesFolderCommand { get; }

        public ICommand ClickTileCommand { get; }

        public ICommand GenerateCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand SaveAsTextCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand HelpCommand { get; }

        public ICommand WhatsNewCommand { get; }

        public ICommand SettingsCommand { get; }

        public ICommand TeachingTipActionButtonCommand { get; }

        public ICommand TeachingTipClosingCommand { get; }

        public ICommand TeachingTipClosedCommand { get; }

        private Size OutputSize => new Size(_outputWidth, _outputHeight);

        private Size TileSize => new Size(_tileWidth, _tileHeight);

        public void PointerEnteredCommandBehavior() => IsAddMasterUIVisible = true;

        public void PointerExitedCommandBehavior() => UpdateIsAddMasterUIVisible();

        public void InitOnboarding(FrameworkElement[] frameworkElements)
        {
            OnboardingHelper.Init(frameworkElements);

            ShowTeachingTip();
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

        public void TeachingTipActionButtonCommandBehavior()
        {
            OnboardingHelper.IsDisabled = true;
            IsTeachingTipOpen = false;
        }

        public void TeachingTipClosingCommandBehavior()
        {
            TeachingTipTitle = "";
            TeachingTipSubTitle = "";
            TeachingTipTarget = null;
            IsTeachingTipOpen = false;
        }

        public void TeachingTipClosedCommandBehavior() => ShowTeachingTip();

        private async Task AddMasterImageCommandBehavior()
        {
            StorageFile masterFile = await FilePickerHelper.OpenFile(
                new List<string> {
                    FileFormat.Jpg.GetStringRepresentation(),
                    FileFormat.Jpeg.GetStringRepresentation(),
                    FileFormat.Png.GetStringRepresentation()
                },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (masterFile != null)
            {
                StartProgressRing(true);

                ResetSizes();

                using (var inputStream = await masterFile.OpenReadAsync())
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                    await dispatcherQueue.EnqueueAsync(async () =>
                    {
                        var bmp = new BitmapImage
                        {
                            DecodePixelHeight = 400,
                            DecodePixelType = DecodePixelType.Logical
                        };
                        MasterBpmSource = bmp;
                        await bmp.SetSourceAsync(inputStream);
                    });
                }

                var masterImageSize = await _mosaicService.AddMasterImage(masterFile);

                Tuple<int, int> newSize = MathHelper.RatioConvert(masterImageSize.Width, masterImageSize.Height, OutputSize.Width, OutputSize.Height);
                OutputWidth = newSize.Item1;
                OutputHeight = newSize.Item2;

                StopProgressRing();
            }

            UpdateIsAddMasterUIVisible();
        }

        private async Task AddTilesCommandBehavior()
        {
            IReadOnlyList<StorageFile> files = await FilePickerHelper.OpenFiles(
                new List<string> {
                    FileFormat.Jpg.GetStringRepresentation(),
                    FileFormat.Jpeg.GetStringRepresentation(),
                    FileFormat.Png.GetStringRepresentation()
                },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (files != null)
            {
                await AddTiles(files);
            }
        }

        private async Task AddTilesFolderCommandBehavior()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
                FileTypeFilter =
                {
                    FileFormat.Jpg.GetStringRepresentation(),
                    FileFormat.Jpeg.GetStringRepresentation(),
                    FileFormat.Png.GetStringRepresentation()
                }
            };

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                await AddFiles(folder);

                IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();
                foreach (var _folder in folderList)
                {
                    await AddFiles(_folder);
                }
            }
        }

        private async Task AddFiles(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            var filteredFiles = files.Where(file => file.FileType.Contains(FileFormat.Jpg.GetStringRepresentation(), StringComparison.InvariantCultureIgnoreCase)
                                                 || file.FileType.Contains(FileFormat.Jpeg.GetStringRepresentation(), StringComparison.InvariantCultureIgnoreCase)
                                                 || file.FileType.Contains(FileFormat.Png.GetStringRepresentation(), StringComparison.InvariantCultureIgnoreCase));
            await AddTiles(filteredFiles);
        }

        private async Task AddTiles(IEnumerable<StorageFile> files)
        {
            StartProgressRing(false);

            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            await Task.Run(() =>
            {
                Parallel.ForEach(files, async file =>
                {
                    try
                    {
                        StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(
                               ThumbnailMode.SingleItem, 120, ThumbnailOptions.None);
                        
                        if(thumbnail.Type == ThumbnailType.Icon)
                        {
                            thumbnail.Dispose();
                            thumbnail = await file.GetThumbnailAsync(
                                ThumbnailMode.SingleItem, 120, ThumbnailOptions.None);
                        }

                        await dispatcherQueue.EnqueueAsync(async () =>
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            TileBmpCollection.Add(new TileBmp(file.DisplayName, bitmapImage));
                            await bitmapImage.SetSourceAsync(thumbnail);
                        });

                        thumbnail.Dispose();

                        _mosaicService.AddTileImage(file.DisplayName, file);
                    }
                    catch (Exception exception)
                    {
                        Crashes.TrackError(exception);
                    }
                });
            });

            StopProgressRing();
        }

        private async Task ClickTileCommandBehavior(TileBmp item)
        {
            await ContentDialogHelper.Confirm(ResourceHelper.GetText("DefaultDeletePicture"), "", ResourceHelper.GetText("DefaultNo"),
                new RelayCommand(() =>
                {
                    TileBmpCollection.Remove(item);
                    _mosaicService.RemoveTileImage(item.Name);
                }), ResourceHelper.GetText("DefaultYes"));
        }

        private async Task GenerateCommandBehavior()
        {
            StartProgressRing(false);

            _progressService.Init(percent =>
            {
                Progress = percent;
            });

            Result<Image<Rgba32>> result;
            result = await Task.Run(async () =>
               await _mosaicService.Generate(OutputSize, TileSize, SelectedMosaicType.MosaicTypeEnum));

            if (result.IsFailure)
            {
                await ContentDialogHelper.Alert(result.Error, "", "Close");
            }
            else
            {
                _outputImage = result.Value;
            }

            if (_outputImage != null)
            {
                InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(_outputImage);
                await OutputBmpSource.SetSourceAsync(outputStream);

                OutputWidth = OutputBmpSource.PixelWidth;
                OutputHeight = OutputBmpSource.PixelHeight;
                IsAlignmentGridVisibile = false;
            }

            StopProgressRing();
        }

        private async Task SaveCommandBehavior()
        {
            StartProgressRing(false);

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
                var fileFormat = EnumHelper.GetValueFromDescription<FileFormat>(file.FileType);
                switch (fileFormat)
                {
                    case FileFormat.Png:
                        _outputImage.SaveAsPng(stream.AsStreamForWrite());
                        break;

                    default:
                        _outputImage.SaveAsJpeg(stream.AsStreamForWrite());
                        break;
                }
            }

            StopProgressRing();
        }

        private async Task SaveAsTextCommandBehavior()
        {
            StartProgressRing(false);

            var fileTypes = new Dictionary<string, List<string>>()
            {
                {FileFormat.Txt.ToString(), new List<string>() {FileFormat.Txt.GetStringRepresentation()}}
            };

            StorageFile file = await FilePickerHelper.SaveFile("Mosaic", fileTypes,
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            var text = _mosaicService.GetAsciiText;

            if (file == null || _outputImage == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            await FileIO.WriteTextAsync(file, text);

            StopProgressRing();
        }

        private void ResetCommandBehavior()
        {
            _mosaicService.Reset();

            MasterBpmSource = new BitmapImage();
            TileBmpCollection = new ObservableCollection<TileBmp>();

            ResetSizes();

            GC.Collect();

            UpdateIsAddMasterUIVisible();
        }

        private void ResetSizes()
        {
            OutputHeight = 1000;
            OutputWidth = 1000;
            TileHeight = 25;
            TileWidth = 25;
        }

        private void HelpCommandBehavior()
        {
            OnboardingHelper.Reset();
            ShowTeachingTip();
        }

        private async Task SettingsCommandBehavior()
        {
            var settingsDialog = new SettingsDialog();
            await settingsDialog.ShowAsync();
        }

        private async Task WhatsNewCommandBehavior()
        {
            var WhatsNewCommandBehavior = new WhatsNewDialog();
            await WhatsNewCommandBehavior.ShowAsync();
        }

        private void UpdateIsAddMasterUIVisible() =>
            IsAddMasterUIVisible = MasterBpmSource.PixelWidth <= 0 || MasterBpmSource.PixelHeight <= 0;

        private void StartProgressRing(bool isIndeterminate)
        {
            IsButtonEnabled = false;
            IsLoading = true;
            IsIndeterminate = isIndeterminate;
        }

        private void StopProgressRing()
        {
            IsLoading = false;
            IsButtonEnabled = true;
        }
    }
}