using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MqttClient
{
    public class HTHistorySDK
    {
        public static string GetString(string url)
        {
            //后台client方式GET提交
            HttpClient myHttpClient = new HttpClient();
            //提交当前地址的webapi
            //myHttpClient.BaseAddress = new Uri(url);
            //GET提交 返回string
            HttpResponseMessage response = myHttpClient.GetAsync(url).Result;
            string result = "";
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }

            return result;
            //return Content(JsonConvert.SerializeObject(result));

            //Product product = null;
            ////GET提交 返回class
            //response = myHttpClient.GetAsync("api/ProductsAPI/GetProduct/1").Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    product = response.Content.ReadAsAsync<Product>().Result;
            //}
            //return Content (JsonConvert.SerializeObject(product));
        }

        public void Put()
        {
            ////put 提交 先创建一个和webapi对应的类           
            //var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            //{
            //    {"Id","2"},
            //    {"Name","Name:"+DateTime.Now.ToString() },
            //    {"Category","111"},
            //    {"Price","1"}
            // });
            //response = myHttpClient.PutAsync("api/ProductsAPI/PutProduct/2", content).Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    result = response.Content.ReadAsStringAsync().Result;
            //}
        }

        public void Post()
        {
            //post 提交 先创建一个和webapi对应的类
            //content = new FormUrlEncodedContent(new Dictionary<string, string>()
            //{
            //    {"Id","382accff-57b2-4d6e-ae84-a61e00a3e3b5"},
            //    {"Name","Name" },
            //    {"Category","111"},
            //    {"Price","1"}
            // });
            //response = myHttpClient.PostAsync("api/ProductsAPI/PostProduct", content).Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    result = response.Content.ReadAsStringAsync().Result;
            //}
        }

        public void Delete()
        {
            //delete 提交
            //response = myHttpClient.DeleteAsync("api/ProductsAPI/DeleteProduct/1").Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    result = response.Content.ReadAsStringAsync().Result;
            //}
        }

        public void GetModel()
        {
            //GET提交 返回List<class>
            //response = myHttpClient.GetAsync("api/ProductsAPI/GetAllProducts").Result;
            //List<Product> listproduct = new List<Models.Product>();
            //if (response.IsSuccessStatusCode)
            //{
            //    listproduct = response.Content.ReadAsAsync<List<Product>>().Result;
            //}
            //return Content(JsonConvert.SerializeObject(listproduct));
        }
    }
}
