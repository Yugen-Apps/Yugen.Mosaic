using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private BitmapImage masterBmpSource;
        public BitmapImage MasterBpmSource
        {
            get { return masterBmpSource; }
            set { Set(ref masterBmpSource, value); }
        }

        private bool isAddMasterUIVisible = true;
        public bool IsAddMasterUIVisible
        {
            get { return isAddMasterUIVisible; }
            set { Set(ref isAddMasterUIVisible, value); }
        }

        private int tileWidth = 50;
        public int TileWidth
        {
            get { return tileWidth; }
            set
            {
                Set(ref tileWidth, value);
                tileSize.Width = tileWidth;
            }
        }

        private int tileHeight = 50;
        public int TileHeight
        {
            get { return tileHeight; }
            set
            {
                Set(ref tileHeight, value);
                tileSize.Height = tileHeight;
            }
        }

        private Size tileSize = new Size(50, 50);

        private ObservableCollection<TileBmp> tileBmpCollection = new ObservableCollection<TileBmp>();
        public ObservableCollection<TileBmp> TileBmpCollection
        {
            get { return tileBmpCollection; }
            set { Set(ref tileBmpCollection, value); }
        }


        private BitmapImage outputBmpSource = new BitmapImage();
        public BitmapImage OutputBmpSource
        {
            get { return outputBmpSource; }
            set { Set(ref outputBmpSource, value); }
        }

        private int outputWidth = 1000;
        public int OutputWidth
        {
            get { return outputWidth; }
            set
            {
                Set(ref outputWidth, value);
                outputSize.Width = outputWidth;
            }
        }

        private int outputHeight = 1000;
        public int OutputHeight
        {
            get { return outputHeight; }
            set
            {
                Set(ref outputHeight, value);
                outputSize.Height = outputHeight;
            }
        }

        private Size outputSize = new Size(1000, 1000);


        public List<MosaicType> MosaicTypeList { get; set; } = new List<MosaicType>
        {
            new MosaicType { Id=0, Title="Classic" },
            new MosaicType { Id=1, Title="AdjustHue" },
            new MosaicType { Id=2, Title="Plain Color" }
        };

        private MosaicType mosaicSelectedType;
        public MosaicType MosaicSelectedType
        {
            get { return mosaicSelectedType; }
            set { Set(ref mosaicSelectedType, value); }
        }
        

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }

        private Image<Rgba32> masterImage;
        private List<Image<Rgba32>> tileImageList = new List<Image<Rgba32>>();
        private Image<Rgba32> outputImage;


        public MainViewModel()
        {
            MosaicSelectedType = MosaicTypeList[0];
        }


        public void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            IsAddMasterUIVisible = true;
        }

        public void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            IsAddMasterUIVisible = (MasterBpmSource != null) ? false : true;
        }

        public void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AddMasterButton_Click(sender, e);
            IsAddMasterUIVisible = false;
        }


        public async void AddMasterButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var masterFile = await FilePickerHelper.OpenFile(new List<string> { ".jpg", ".png" });
            if (masterFile == null)
                return;

            using (var inputStream = await masterFile.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                masterImage = Image.Load<Rgba32>(stream);

                var resizeOptions = new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.Primitives.Size(400, 400)
                };

                using (Image copy = masterImage.Clone(x => x.Resize(resizeOptions)))
                {
                    InMemoryRandomAccessStream outputStream = new InMemoryRandomAccessStream();
                    copy.SaveAsJpeg(outputStream.AsStreamForWrite());

                    outputStream.Seek(0);
                    MasterBpmSource = new BitmapImage();
                    await MasterBpmSource.SetSourceAsync(outputStream);
                }
            }
        }

        public async void AddTilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var files = await FilePickerHelper.OpenFiles(new List<string> { ".jpg", ".png" });
            if (files == null)
                return;

            IsLoading = true;

            foreach (var file in files)
            {
                Image<Rgba32> image;
                using (var inputStream = await file.OpenReadAsync())
                using (var stream = inputStream.AsStreamForRead())
                {
                    image = Image.Load<Rgba32>(stream);
                    tileImageList.Add(image);

                    var resizeOptions = new ResizeOptions() { 
                        Mode = ResizeMode.Max ,
                        Size = new SixLabors.Primitives.Size(200,200)
                    };

                    using (Image copy = image.Clone(x => x.Resize(resizeOptions)))
                    {
                        InMemoryRandomAccessStream outputStream = new InMemoryRandomAccessStream();
                        copy.SaveAsJpeg(outputStream.AsStreamForWrite());

                        outputStream.Seek(0);
                        var bmp = new BitmapImage();
                        await bmp.SetSourceAsync(outputStream);

                        var tileBmp = new TileBmp(file.Name, bmp);
                        TileBmpCollection.Add(tileBmp);
                    }
                }
            }

            IsLoading = false;
        }

        public async void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IsLoading = true;

            outputImage = await Generate();

            if (outputImage != null)
            {
                //var bmp = await WriteableBitmapHelper.ImageToWriteableBitmap(outputImage);
                InMemoryRandomAccessStream outputStream = new InMemoryRandomAccessStream();
                outputImage.SaveAsJpeg(outputStream.AsStreamForWrite());

                outputStream.Seek(0);
                await OutputBmpSource.SetSourceAsync(outputStream);
            }

            IsLoading = false;
        }

        private async Task<Image<Rgba32>> Generate()
        {
            await Task.Delay(1);

            if (masterImage == null || tileImageList.Count < 1)
                return null;

            Image<Rgba32> resizedMasterImage = masterImage.Clone(x => x.Resize(outputWidth, outputHeight));
            MosaicService newMosaicClass = new MosaicService();
            var newOutputSize = new SixLabors.Primitives.Size((int)outputSize.Width, (int)outputSize.Height);
            var newTileSize = new SixLabors.Primitives.Size((int)tileSize.Width, (int)tileSize.Height);
            return newMosaicClass.GenerateMosaic(resizedMasterImage, newOutputSize, tileImageList, newTileSize, MosaicSelectedType.Id);
        }

        public async void SaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fileFormat = FileFormat.Jpg;
            var file = await FilePickerHelper.SaveFile("Mosaic", "Image", fileFormat.FileFormatToString());
            if (file == null)
                return;

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                outputImage.SaveAsJpeg(stream.AsStreamForWrite());
            }

            //await WriteableBitmapHelper.WriteableBitmapToStorageFile(file, outputBmpSource, fileFormat);
        }

        public void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MasterBpmSource = null;
            masterImage = null;

            TileBmpCollection.Clear();
            tileImageList.Clear();
        }
    }
}
