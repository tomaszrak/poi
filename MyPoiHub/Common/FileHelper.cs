using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MyPoiHub.Common;
using System.IO;
using Windows.Storage.Streams;
using MyPoiHub.Data;
using System.Runtime.Serialization.Json;
using Windows.Web.Http;




namespace MyPoiHub.Common
{
    class FileHelper
    {
        static FileHelper _fileHelper;
        private static DataSource _dataSource = new DataSource();
        public static StorageFile file { get; set; }
        public static StorageFolder folder = ApplicationData.Current.LocalFolder;
        public static String jsonText { get; set; }

        public static async Task<FileHelper> GetInstace()
        {
            if (_fileHelper == null)
            {
                _fileHelper = new FileHelper();
                await _fileHelper.GetFileAsync();
            }
            return _fileHelper;
        }
        static FileHelper()
        {

        }
        public async Task<FileHelper> GetFileAsync()
        {
            
            Uri dataUri2 = new Uri("http://mypoi.ugu.pl/poi.json");
            if (await DoesFileExistAsync("poi.json"))
            {
                file = await folder.GetFileAsync("poi.json");
            }
            else
            {
                file = await folder.CreateFileAsync("poi.json", CreationCollisionOption.OpenIfExists);
            }

            var httpClient = new HttpClient();

            try
            {
                string result = await httpClient.GetStringAsync(dataUri2); 
                //MessageDialogHelper.Show(result, "");
                await WriteFileAsync(result);
            }
            catch
            {

            }
            return this;
        }
        async Task<bool> DoesFileExistAsync(string fileName) 
        {
	        try 
            {
                await folder.GetFileAsync(fileName);
		        return true;
	        } 
            catch 
            {
		        return false;
	        }
        }
        public async Task<String> ReadFileAsync()
        {
            try
            {
                try
                {
                    using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    {
                        jsonText = await sRead.ReadToEndAsync();
                    }
                }
                catch (System.Exception e)
                {
                    MessageDialogHelper.Show(e.Message, e.Source);
                }
            }
            catch (System.UnauthorizedAccessException e)
            {
                MessageDialogHelper.Show(e.Message, e.Source);
            }
            return jsonText;
        }
        public async Task WriteFileAsync(string jsonString)
        {
            StorageFile copyTemp = await folder.CreateFileAsync("temp.json", CreationCollisionOption.ReplaceExisting);
            try
            {
                try
                {
                    try
                    {
                        //MessageDialogHelper.Show(jsonString, "");
                        using (StreamWriter sWriter = new StreamWriter(await copyTemp.OpenStreamForWriteAsync()))
                        {
                            await sWriter.WriteAsync(jsonString);
                        }
                    }
                    catch (System.Exception e)
                    {
                        MessageDialogHelper.Show(e.Message, e.Source);
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    MessageDialogHelper.Show(e.Message, e.Source);
                }

            }
            catch (System.UnauthorizedAccessException e)
            {
                MessageDialogHelper.Show(e.Message, e.Source);
            }
            await copyTemp.CopyAndReplaceAsync(file);

            IInputStream inputStream = await file.OpenAsync(FileAccessMode.Read);
            HttpMultipartFormDataContent multipartContent = new HttpMultipartFormDataContent();
            multipartContent.Add(new HttpStreamContent(inputStream), "myFile", file.Name);
            
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(new Uri("http://mypoi.ugu.pl/main.php"), multipartContent);
        }
    }
}