using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
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
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Controls;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
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
        private bool _isIndeterminateLoading;
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
        private ICommand _pointerEnteredCommand;
        private ICommand _pointerExitedCommand;
        private ICommand _addMasterImmageCommand;
        private ICommand _addTilesCommand;
        private ICommand _addTilesFolderCommand;
        private ICommand _clickTileCommand;
        private ICommand _generateCommand;
        private ICommand _saveCommand;
        private ICommand _resetCommand;
        private ICommand _helpCommand;
        private ICommand _settingsCommand;
        private ICommand _teachingTipActionButtonCommand;
        //private ICommand _teachingTipClosingCommand;
        //private ICommand _teachingTipClosedCommand;

        public MainViewModel(IMosaicService mosaicService, IProgressService progressService)
        {
            SelectedMosaicType = MosaicTypeList[0];
            _mosaicService = mosaicService;
            _progressService = progressService;
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

        public bool IsIndeterminateLoading
        {
            get => _isIndeterminateLoading;
            set => SetProperty(ref _isIndeterminateLoading, value);
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
            set => SetProperty(ref _selectedMosaicType, value);
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

        public ICommand PointerEnteredCommand => _pointerEnteredCommand ?? (_pointerEnteredCommand = new RelayCommand(PointerEnteredCommandBehavior));
        public ICommand PointerExitedCommand => _pointerExitedCommand ?? (_pointerExitedCommand = new RelayCommand(PointerExitedCommandBehavior));
        public ICommand AddMasterImmageCommand => _addMasterImmageCommand ?? (_addMasterImmageCommand = new AsyncRelayCommand(AddMasterImmageCommandBehavior));
        public ICommand AddTilesCommand => _addTilesCommand ?? (_addTilesCommand = new AsyncRelayCommand(AddTilesCommandBehavior));
        public ICommand AddTilesFolderCommand => _addTilesFolderCommand ?? (_addTilesFolderCommand = new AsyncRelayCommand(AddTilesFolderCommandBehavior));
        public ICommand ClickTileCommand => _clickTileCommand ?? (_clickTileCommand = new AsyncRelayCommand<TileBmp>(ClickTileCommandBehavior));
        public ICommand GenerateCommand => _generateCommand ?? (_generateCommand = new AsyncRelayCommand(GenerateCommandBehavior));
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new AsyncRelayCommand(SaveCommandBehavior));
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(ResetCommandBehavior));
        public ICommand HelpCommand => _helpCommand ?? (_helpCommand = new RelayCommand(HelpCommandBehavior));
        public ICommand SettingsCommand => _settingsCommand ?? (_settingsCommand = new AsyncRelayCommand(SettingsCommandBehavior));
        public ICommand TeachingTipActionButtonCommand => _teachingTipActionButtonCommand ?? (_teachingTipActionButtonCommand = new RelayCommand(TeachingTipActionButtonCommandBehavior));
        //public ICommand TeachingTipClosingCommand => _teachingTipClosingCommand ?? (_teachingTipClosingCommand = new RelayCommand(TeachingTipClosingCommandBehavior));
        //public ICommand TeachingTipClosedCommand => _teachingTipClosedCommand ?? (_teachingTipClosedCommand = new RelayCommand(TeachingTipClosedCommandBehavior));

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

        public void TeachingTip_Closing(TeachingTip sender, TeachingTipClosingEventArgs args)
        {
            TeachingTipTitle = "";
            TeachingTipSubTitle = "";
            TeachingTipTarget = null;
            IsTeachingTipOpen = false;
        }

        public void TeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args) => ShowTeachingTip();

        private async Task AddMasterImmageCommandBehavior()
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
                IsButtonEnabled = false;
                IsIndeterminateLoading = true;

                using (var inputStream = await masterFile.OpenReadAsync())
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    {
                        var bmp = new BitmapImage
                        {
                            DecodePixelHeight = 400
                        };
                        await bmp.SetSourceAsync(inputStream);
                        MasterBpmSource = bmp;
                    });
                }

                var masterImageSize = await _mosaicService.AddMasterImage(masterFile);

                Tuple<int, int> newSize = MathHelper.RatioConvert(masterImageSize.Width, masterImageSize.Height, OutputSize.Width, OutputSize.Height);
                OutputWidth = newSize.Item1;
                OutputHeight = newSize.Item2;

                IsIndeterminateLoading = false;
                IsButtonEnabled = true;
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
            IsButtonEnabled = false;
            IsIndeterminateLoading = true;

            await Task.Run(() =>
                Parallel.ForEach(files, async file =>
                {
                    try
                    {
                        using (var inputStream = await file.OpenReadAsync())
                        {
                            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                            {
                                var bmp = new BitmapImage
                                {
                                    DecodePixelHeight = 120
                                };
                                await bmp.SetSourceAsync(inputStream);

                                TileBmpCollection.Add(new TileBmp(file.DisplayName, bmp));
                            });
                        }

                        _mosaicService.AddTileImage(file.DisplayName, file);
                    }
                    catch (Exception exception)
                    {
                        Crashes.TrackError(exception);
                    }
                })
            );

            IsIndeterminateLoading = false;
            IsButtonEnabled = true;
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
            IsButtonEnabled = false;
            IsLoading = true;

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

            IsLoading = false;
            IsButtonEnabled = true;
        }

        private async Task SaveCommandBehavior()
        {
            IsButtonEnabled = false;
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

            IsLoading = false;
            IsButtonEnabled = true;
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

        private void UpdateIsAddMasterUIVisible() =>
            IsAddMasterUIVisible = MasterBpmSource.PixelWidth <= 0 || MasterBpmSource.PixelHeight <= 0;
    }
}