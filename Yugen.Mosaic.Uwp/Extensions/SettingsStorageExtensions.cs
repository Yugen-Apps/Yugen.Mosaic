﻿//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Windows.Storage;
//using Windows.Storage.Streams;
//using Yugen.Toolkit.Standard.Providers;

//namespace Yugen.Mosaic.Uwp.Extensions
//{

//    public static class SettingsStorageExtensions
//    {
//        private const string FileExtension = ".json";

//        public static bool IsRoamingStorageAvailable(this ApplicationData appData)
//        {
//            return appData.RoamingStorageQuota == 0;
//        }

//        public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
//        {
//            var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
//            var fileContent = await JsonProvider.StringifyAsync(content);

//            await FileIO.WriteTextAsync(file, fileContent);
//        }

//        public static async Task<T> ReadAsync<T>(this StorageFolder folder, string name)
//        {
//            if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
//            {
//                return default;
//            }

//            var file = await folder.GetFileAsync($"{name}.json");
//            var fileContent = await FileIO.ReadTextAsync(file);

//            return await JsonProvider.ToObjectAsync<T>(fileContent);
//        }



//        public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
//        {
//            if (content == null)
//            {
//                throw new ArgumentNullException(nameof(content));
//            }

//            if (string.IsNullOrEmpty(fileName))
//            {
//                throw new ArgumentException("ExceptionSettingsStorageExtensionsFileNameIsNullOrEmpty", nameof(fileName));
//            }

//            var storageFile = await folder.CreateFileAsync(fileName, options);
//            await FileIO.WriteBytesAsync(storageFile, content);
//            return storageFile;
//        }

//        public static async Task<byte[]> ReadFileAsync(this StorageFolder folder, string fileName)
//        {
//            var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

//            if (item != null && item.IsOfType(StorageItemTypes.File))
//            {
//                var storageFile = await folder.GetFileAsync(fileName);
//                byte[] content = await storageFile.ReadBytesAsync();
//                return content;
//            }

//            return null;
//        }

//        public static async Task<byte[]> ReadBytesAsync(this StorageFile file)
//        {
//            if (file != null)
//            {
//                using (IRandomAccessStream stream = await file.OpenReadAsync())
//                {
//                    using (var reader = new DataReader(stream.GetInputStreamAt(0)))
//                    {
//                        await reader.LoadAsync((uint)stream.Size);
//                        var bytes = new byte[stream.Size];
//                        reader.ReadBytes(bytes);
//                        return bytes;
//                    }
//                }
//            }

//            return null;
//        }

//        private static string GetFileName(string name)
//        {
//            return string.Concat(name, FileExtension);
//        }
//    }
//}
