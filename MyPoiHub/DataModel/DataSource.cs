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
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace MyPoiHub.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class DataItem
    {
        public DataItem(String uniqueId, String title, String subtitle, String imagePath, Double latitude, Double longitude)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.ImagePath = imagePath;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string ImagePath { get; private set; }
        public double Latitude { get; private set; }    
        public double Longitude { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    ///
    public class DataGroup
    {
        public DataGroup(String uniqueId, String title, String imagePath)
        {
            this.ImagePath = imagePath;
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Items = new ObservableCollection<DataItem>();
        }
        public string ImagePath { get; private set; }
        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public ObservableCollection<DataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// DataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class DataSource
    {
        private static DataSource _dataSource = new DataSource();
        private ObservableCollection<DataGroup> _groups = new ObservableCollection<DataGroup>();
        public ObservableCollection<DataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<DataGroup>> GetGroupsAsync()
        {
            await _dataSource.GetDataAsync();

            return _dataSource.Groups;
        }

        public static async Task<DataGroup> GetGroupAsync(string uniqueId)
        {
            await _dataSource.GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _dataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<DataItem> GetItemAsync(string uniqueId)
        {
            await _dataSource.GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _dataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task SetDataAsync(DataItem dataItem, int iType, String sType)
        {
            FileHelper _fileHelper = await FileHelper.GetInstace();

            JsonObject itemObject = new JsonObject();
            itemObject.Add("UniqueId", JsonValue.CreateStringValue(dataItem.Title + " " + dataItem.Subtitle));
            itemObject.Add("Title", JsonValue.CreateStringValue(dataItem.Title));
            itemObject.Add("Subtitle", JsonValue.CreateStringValue(dataItem.Subtitle));
            itemObject.Add("Type", JsonValue.CreateStringValue(_dataSource._groups[iType].UniqueId));
            itemObject.Add("ImagePath", JsonValue.CreateStringValue(_dataSource._groups[iType].ImagePath));
            itemObject.Add("Latitude", JsonValue.CreateNumberValue(dataItem.Latitude));
            itemObject.Add("Longitude", JsonValue.CreateNumberValue(dataItem.Longitude));
            
            _dataSource._groups[iType].Items.Add(new DataItem(itemObject["UniqueId"].GetString(),
                                                               itemObject["Title"].GetString(),
                                                               itemObject["Subtitle"].GetString(),
                                                               itemObject["ImagePath"].GetString(),
                                                               itemObject["Latitude"].GetNumber(),
                                                               itemObject["Longitude"].GetNumber()));
            string jsonString2 = JsonConvert.SerializeObject(_dataSource, Formatting.Indented);
            await _fileHelper.WriteFileAsync(jsonString2);    
        }     
        private async Task GetDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            FileHelper _fileHelper = await FileHelper.GetInstace();
            string jsonText = await _fileHelper.ReadFileAsync();
           
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();
            
            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                DataGroup group = new DataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["ImagePath"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new DataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Latitude"].GetNumber(),
                                                       itemObject["Longitude"].GetNumber()));
                }
                this.Groups.Add(group); 
            }
        }
    }
}