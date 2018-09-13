using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Implementations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace EmqMongoConnect
{
    class Program
    {
        private static IList<IMqttClient> _mqttClients = new List<IMqttClient>();
        private static readonly RTContext _context = new RTContext();
        private static int _maxTask = 5;
        private static ConcurrentBag<Task> _runningTasks = new ConcurrentBag<Task>();
        private static LimitedConcurrencyLevelTaskScheduler _scheduler = new LimitedConcurrencyLevelTaskScheduler(_maxTask);
        static void Main(string[] args)
        {
            int i = 0;
            do
            {
                try
                {
                    //Task.Factory.StartNew(() =>
                    //{
                    var factory = new MqttFactory();
                    var mqttClient = factory.CreateMqttClient();
                    var options = new MqttClientOptionsBuilder()
                    .WithClientId($"collection.A{i}.{Guid.NewGuid().ToString()}")
                    .WithTcpServer("127.0.0.1", 1883)
                    //.WithTcpServer("118.24.180.83", 1883)
                    //.WithTcpServer("132.232.98.119", 1883)
                    .WithCredentials("admin", "public")
                    .Build();
                    //连接断了后重新连
                    mqttClient.Disconnected += async (s, e) =>
                    {
                        //Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                        await Task.Delay(TimeSpan.FromSeconds(5));

                        try
                        {
                            await mqttClient.ConnectAsync(options);
                        }
                        catch
                        {
                            Console.WriteLine("### RECONNECTING FAILED ###");
                        }
                    };
                    mqttClient.ApplicationMessageReceived += (s, e) =>
                    {
                        //接受到消息并处理
                        //Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                        //Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                        //Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                        //Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                        //Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                        if (!"collection".Equals(e.ApplicationMessage.Topic))
                            return;

                        try
                        {
                            var receivedJsonString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            var isJsonString = JsonSplit.IsJson(receivedJsonString);
                            if (isJsonString)
                            {
                                var jObject = JObject.Parse(receivedJsonString);
                                InsertBson(jObject.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    };
                    mqttClient.Connected += async (s, e) =>
                    {
                        //Console.WriteLine("### CONNECTED WITH SERVER ###");
                        //连接成功后马上订阅消息,主题是topic3
                        await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(@"$share/group/collection").Build());
                        //Console.WriteLine("### SUBSCRIBED ###");
                    };

                    //建立连接
                    mqttClient.ConnectAsync(options);
                    //});
                    //Console.WriteLine(Thread.CurrentThread.ManagedThreadId+" EmqMongoConnect is running...");
                    _mqttClients.Add(mqttClient);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Thread.Sleep(0);
                }
            } while (++i <3);

            Console.WriteLine("EmqMongoConnect is running...");

            while (true)
            {
                try
                {
                    //创建约束生成器
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    //约束条件
                    //FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "jack36");
                    FilterDefinition<BsonDocument> filter = builderFilter.Eq("isactive", 1);
                    //获取数据
                    var tenantResult = _context.RTTenantsBson.Find<BsonDocument>(filter).ToList();
                    //var tenantResult = _context.TenantsBson.Find(new BsonDocument()).ToList();
                    foreach (var item in tenantResult)
                    {
                        FilterDefinition<BsonDocument> pointFilter = builderFilter.Eq("isactive", 1) & builderFilter.Eq("tenantid", item["tenantid"].ToString()) & builderFilter.Eq("datastype.value", "double") & builderFilter.Eq("pointtype", 0);
                        var pointBsonResult = _context.RTPointsBson.Find<BsonDocument>(pointFilter).ToList();
                        foreach (var pointitem in pointBsonResult)
                        {
                            DoTask(pointitem, _scheduler);
                        }
                    }

                    Task.WaitAll(_runningTasks.ToArray());
                    if (_runningTasks.Count > 0)
                        Thread.Sleep(10);
                    else
                        Thread.Sleep(2000);
                    _runningTasks.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Process MongoTransfer " + ex.Message);
                }
                finally
                {
                    Thread.Sleep(0);
                }
            }
        }

        public static void InsertBson(string json)
        {
            var nowTimestampServer = GetTimestamp(DateTime.Now);
            BsonDocument newHeartbeatPoint = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
            newHeartbeatPoint.Add(new BsonElement("timestampserver",nowTimestampServer));
            var pointid=newHeartbeatPoint["pointid"];
            var tenantid = newHeartbeatPoint["tenantid"];
            var pointCollection = _context.RTPointsBson;
            var builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            var filter = builderFilter.Eq("isactive", 1)& builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid);
            //获取数据
            var pointResult =pointCollection.Find<BsonDocument>(filter).FirstOrDefault();
            var rtCollectionName = pointResult["storage"]["rtcollection"].ToString();
            var pointtype = Convert.ToDouble(pointResult["pointtype"]);
            var rtCollection = _context.RTDatabase.GetCollection<BsonDocument>(rtCollectionName);
            var filterHeartBeat = builderFilter.Eq("tenantid", tenantid) & builderFilter.Eq("pointid", pointid);
            var oldHeartbeatPoint = rtCollection.Find<BsonDocument>(filterHeartBeat).Sort(Builders<BsonDocument>.Sort.Descending("timestampserver")).FirstOrDefault();
            if (pointtype == 0)//心跳包
            {
                if (oldHeartbeatPoint == null || 1 != Convert.ToInt32(oldHeartbeatPoint["value"]))
                {
                    rtCollection.InsertOne(newHeartbeatPoint);
                }
                else
                {
                    //rtCollection.InsertOne(newHeartbeatPoint);
                    //rtCollection.DeleteMany(Builders<BsonDocument>.Filter.Eq("pointid", pointid) 
                    //    & Builders<BsonDocument>.Filter.Eq("tenantid", tenantid)
                    //    & Builders<BsonDocument>.Filter.Lt("timestampserver",nowTimestampServer));
                    var updateResult=rtCollection.UpdateOne(Builders<BsonDocument>.Filter.Eq("tenantid", oldHeartbeatPoint["tenantid"].ToString()) 
                        &Builders<BsonDocument>.Filter.Eq("pointid", oldHeartbeatPoint["pointid"].ToString())
                        & Builders<BsonDocument>.Filter.Eq("timestampclient",Convert.ToUInt64(oldHeartbeatPoint["timestampclient"]))
                        & Builders<BsonDocument>.Filter.Eq("timestampserver", Convert.ToUInt64(oldHeartbeatPoint["timestampserver"])),
                        Builders<BsonDocument>.Update.Set("timestampclient", nowTimestampServer).Set("timestampserver", nowTimestampServer));
                }
            }
            else
            {
                rtCollection.InsertOne(newHeartbeatPoint);
            }
        }

        static void DoTask(BsonDocument pointitem, TaskScheduler scheduler)
        {
            Action<object> act = obj =>
            {
                var isactive = Convert.ToInt32(pointitem["isactive"]);
                if (isactive == 0)
                    return;

                var pointid = pointitem["pointid"].ToString();
                var interval = pointitem["interval"].ToString();
                var pointtype = pointitem["pointtype"].ToString();
                var iscompress = Convert.ToDouble(pointitem["compress"]["iscompress"]);
                var compressmethod = pointitem["compress"]["compressmethod"].ToString();
                var compresscycle = Convert.ToDouble(pointitem["compress"]["compresscycle"]);
                var accuracy = Convert.ToDouble(pointitem["compress"]["accuracy"]);
                var rtcollection = pointitem["storage"]["rtcollection"].ToString();
                var tenantid = pointitem["tenantid"].ToString();
                var historycollection = pointitem["storage"]["historycollection"].ToString();

                if ("0".Equals(pointtype))//心跳包
                {
                    var nowTimestampServer = GetTimestamp(DateTime.Now);
                    var sb = new StringBuilder();
                    sb.Append("{'pointid':'");
                    sb.Append(pointid);
                    sb.Append("',");
                    sb.Append("'timestampclient':");
                    sb.Append(nowTimestampServer);
                    sb.Append(",");
                    sb.Append("'interval':");
                    sb.Append(interval);
                    sb.Append(",");
                    sb.Append("'value':");
                    sb.Append(0);
                    sb.Append(",");
                    sb.Append("'quality':");
                    sb.Append(1);
                    sb.Append(",");
                    sb.Append("'tenantid':'");
                    sb.Append(tenantid);
                    sb.Append("',");
                    sb.Append("'timestampserver':");
                    sb.Append(nowTimestampServer);
                    sb.Append("}");
                    var newHeartbeatPoint = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(sb.ToString());
                    var rtHeartbeatPointCollection = _context.RTPointDatasBson(rtcollection);
                    var rtHeartbeatPointOnLine = rtHeartbeatPointCollection.Find<BsonDocument>(Builders<BsonDocument>.Filter.Eq("pointid", pointid) & Builders<BsonDocument>.Filter.Eq("tenantid", tenantid)).Sort(Builders<BsonDocument>.Sort.Descending("timestampserver")).FirstOrDefault();
                    
                    //实时库里没有心跳包
                    if (rtHeartbeatPointOnLine == null)
                    {
                        //rtHeartbeatPointCollection.InsertOne(newHeartbeatPoint);
                        return;
                    }
                    //实时库里心跳包时间离现在时间>interval*5=离线
                    else if(0!= Convert.ToInt32(rtHeartbeatPointOnLine["value"])&& (nowTimestampServer-Convert.ToInt64(rtHeartbeatPointOnLine["timestampserver"])>Math.Max(10000,Convert.ToDouble(rtHeartbeatPointOnLine["interval"]) * 5)))
                    {
                        newHeartbeatPoint["value"] = 0;
                        rtHeartbeatPointCollection.InsertOne(newHeartbeatPoint);
                    }
                }
            };

            var task = Task.Factory.StartNew(act, pointitem, CancellationToken.None, TaskCreationOptions.None, scheduler).ContinueWith((t, obj) =>
            {
                if (t.Status != TaskStatus.RanToCompletion)
                {
                    DoTask(pointitem, scheduler);
                }
                //Console.WriteLine(obj);
            }, pointitem);
            _runningTasks.Add(task);
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
}
