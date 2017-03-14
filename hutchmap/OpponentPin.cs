using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace hutchmap
{
    public class PinData
    {
        string id;
        string username;
        bool done;

        double opLat;
        double opLong;

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [JsonProperty(PropertyName = "text")]
        public string UserName
        {
            get { return username; }
            set { username = value; }
        }

        [JsonProperty(PropertyName = "complete")]
        public bool Done
        {
            get { return done; }
            set { done = value; }
        }

        [JsonProperty(PropertyName = "opLong")]
        public double OpLong
        {
            get { return opLong; }
            set { opLong = value; }
        }

        [JsonProperty(PropertyName = "opLat")]
        public double OpLat
        {
            get { return opLat; }
            set { opLat = value; }
        }

        [Version]
        public string Version { get; set; }
    }
}
