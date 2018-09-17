using EmqMongoConnect;
using MongoDB.Bson;
using MongoDB.Driver;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Threading.Tasks;

namespace MqttClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var factory = new MqttFactory();
            //var mqttClient = factory.CreateMqttClient();
            //var options = new MqttClientOptionsBuilder()
            //.WithClientId("dotnetclient1")
            ////.WithTcpServer("132.232.98.119", 1883)
            //.WithTcpServer("118.24.180.83", 1883)
            //.WithCredentials("admin", "public")
            //.Build();
            ////连接断了后重新连
            //mqttClient.Disconnected += async (s, e) =>
            //{
            //    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            //    await Task.Delay(TimeSpan.FromSeconds(5));

            //    try
            //    {
            //        await mqttClient.ConnectAsync(options);
            //    }
            //    catch (Exception ee)
            //    {
            //        Console.WriteLine("### RECONNECTING FAILED ###");
            //    }
            //};
            //mqttClient.ApplicationMessageReceived += (s, e) =>
            //{
            //    //接受到消息并处理
            //    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            //    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            //    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            //    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            //    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            //    Console.WriteLine();
            //    var now = DateTime.Now;
            //    System.Diagnostics.Debug.WriteLine(now.ToLocalTime() + "." + now.Millisecond + "--->" + $"Message from dotnet client:+{ Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            //};
            //mqttClient.Connected += async (s, e) =>
            //{
            //    Console.WriteLine("### CONNECTED WITH SERVER ###");
            //    //连接成功后马上订阅消息,主题是topic3
            //    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(@"$local/topic3").Build());
            //    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(@"$queue/topic3").Build());

            //    Console.WriteLine("### SUBSCRIBED ###");

            //    Task.Factory.StartNew(() =>
            //    {
            //        while (true)
            //        {
            //            for (int i = 0; i < 10000; i++)
            //            {
            //                Publich(mqttClient,i).GetAwaiter().GetResult();
            //                //Task.Delay(1000).GetAwaiter().GetResult();
            //                Thread.Sleep(10);
            //            }
            //        }
            //    });

            //    ////并马上publish一个消息，主题是topic1
            //    //var message = new MqttApplicationMessageBuilder()
            //    //.WithTopic("topic1")
            //    //.WithPayload("Message from dotnet client")
            //    //.WithAtMostOnceQoS()
            //    //.Build();
            //    //await mqttClient.PublishAsync(message);
            //};
            ////建立连接
            //mqttClient.ConnectAsync(options);

            Console.WriteLine("Client...");
            Console.Read();
        }

        public static Task Publich(IMqttClient mqttClient,int i)
        {
            //Query();
            var tenantJson = @"
                                {
	                                'pointid':'0',
                                        'pointname':'0',
                                        'interval':200,
                                        'device':{
		                                'deviceid':'deviceid',
		                                'devicename':'devicename',
                                                'position':'65,56',
		                                'area':{
			                                'areaid':'',
                                                        'areaname':''
		                                }
	                                }
	
                                }";

            var json = @"
                {
                    'tenant':'0001',
                    'collection':'data',
                    'message':{
                            'pointid':'0002',
	                        'timestamp':159320987775288,
	                        'tags': {
                                'interval':200,
                                'instance': '172.14 .200 .10',
		                        'service': 'print'
                            },
	                        'fileds': {
                                'averragebytesin': 38.304,
		                        'averagebytesout': 2974.45
                            }
                    }
                }";

            json=json.Replace("0002",i.ToString());
            //var jObject = JObject.Parse(json);
            //var tenant = jObject["tenant"].ToString();
            //var time = jObject["message"]["timestamp"];
            //var pointid = jObject["message"]["pointid"];

            //并马上publish一个消息，主题是topic1
            var message = new MqttApplicationMessageBuilder()
            .WithTopic("collection")//表示采集
            //.WithPayload("Message from dotnet client------"+ _count++)
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
            return mqttClient.PublishAsync(message);
        }

        private static void Query()
        {
            //创建约束生成器
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            var time = new BsonDateTime(Convert.ToDateTime("2017-01-01T00:00:00"));
            //约束条件
            FilterDefinition<BsonDocument> filter = builderFilter.Gte("timestamp", time);
            //获取数据
            var result = new RTContext().ProvinceBson.Find<BsonDocument>(filter).ToList();
            foreach (var item in result)
            {
                //取出整条值
                Console.WriteLine(item.AsBsonValue);
            }
        }

        /// <summary>
        /// 获取1970-01-01至dateTime的毫秒数
        /// </summary>
        public static long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }

        /// <summary>
        /// 根据时间戳timestamp（单位毫秒）计算日期
        /// </summary>
        public static DateTime NewDate(long timestamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = dt1970.Ticks + timestamp * 10000;
            return new DateTime(t);
        }
    }

    //public interface IMongoDBContext
    //{
    //    /// <summary>
    //    /// register entity type
    //    /// </summary>
    //    /// <param name="registration"></param>
    //    void OnRegisterModel(ITypeRegistration registration);

    //    /// <summary>
    //    /// name of ConnectionString in config file
    //    /// </summary>
    //    string ConnectionStringName { get; }

    //    /// <summary>
    //    /// build Configuration by config file
    //    /// </summary>
    //    /// <returns></returns>
    //    IConfigurationRegistration BuildConfiguration();
    //}
    //public interface IEntity
    //{
    //    /// <summary>
    //    /// mongo id
    //    /// </summary>
    //    string Id { get; set; }
    //    /// <summary>
    //    /// save document
    //    /// </summary>
    //    void Save();
    //    /// <summary>
    //    /// remove document
    //    /// </summary>
    //    void Remove();
    //}
}
