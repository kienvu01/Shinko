using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelsApp
{
    public class InventoryAdjustments
    {
        public IConfiguration config;

        public InventoryAdjustments(IConfiguration _config)
        {
            config = _config;
        }
        public  ResponseData inventoryadjustments_create(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData inventoryadjustments_update(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public async Task<List<string>> GetLocationId()
        {
            List<string> re = new List<string>();
            try
            {
                var token = config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/locations.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject d = JObject.Parse(res.data);
                JArray arrLocation = (JArray)d["locations"];
                for (int i = 0; i < arrLocation.Count; i++)
                {
                    var location = (JObject)arrLocation[i];
                    string id = (string)location["id"] ?? "";
                    re.Add(id);
                }
                return re;
            }
            catch (Exception e)
            {
                return re;
            }

        }
    }
}
