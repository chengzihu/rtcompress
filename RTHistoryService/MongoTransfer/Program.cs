using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using static MongoTransfer.CompressUtility;

namespace MongoTransfer
{
    class Program
    {
        private static readonly RTContext _context = new RTContext();
        private static int _maxTask = 5;
        private static ConcurrentBag<Task> _runningTasks = new ConcurrentBag<Task>();
        private static LimitedConcurrencyLevelTaskScheduler _scheduler = new LimitedConcurrencyLevelTaskScheduler(_maxTask);
        static void Main(string[] args)
        {
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
                        FilterDefinition<BsonDocument> pointFilter = builderFilter.Eq("isactive", 1) & builderFilter.Eq("tenantid", item["tenantid"].ToString());// & builderFilter.Eq("datastype.value", "double")& builderFilter.Eq("pointtype",2);
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
                    Console.WriteLine("Process MongoTransfer "+ ex.Message);
                }
                finally
                {
                    Thread.Sleep(0);
                }
            }
        }

        static void DoTask(BsonDocument pointitem, TaskScheduler scheduler)
        {
            Action<object> act = obj =>
            {
                try
                {
                    var isactive = Convert.ToDouble(pointitem["isactive"]);
                    //非激活点不处理
                    if (isactive == 0)
                        return;

                    var tenantid = pointitem["tenantid"].ToString();
                    var pointid = pointitem["pointid"].ToString();
                    var interval = Convert.ToDouble(pointitem["interval"]);
                    var pointtype = Convert.ToInt32(pointitem["pointtype"]);
                    var iscompress = Convert.ToInt32(pointitem["compress"]["iscompress"]);
                    var compressmethod = pointitem["compress"]["compressmethod"].ToString();
                    var compresscycle = Convert.ToDouble(pointitem["compress"]["compresscycle"]);
                    var accuracy = Convert.ToDouble(pointitem["compress"]["accuracy"]);
                    var rtcollection = pointitem["storage"]["rtcollection"].ToString();
                    var historycollection = pointitem["storage"]["historycollection"].ToString();

                    SortDefinitionBuilder<BsonDocument> builderSort = Builders<BsonDocument>.Sort;
                    //排序约束   Ascending 正序    Descending 倒序
                    //SortDefinition<BsonDocument> sort = builderSort.Ascending("timestampclient");
                    //创建约束生成器
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid);

                    var nowTimestamp = GetTimestamp(DateTime.Now);
                    if (0 == pointtype)//心跳包
                    {
                        var firstPointDataBsonOneResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Ascending("timestampserver")).FirstOrDefault();
                        if (firstPointDataBsonOneResult == null)
                            return;

                        var firstTimestamp = (long)firstPointDataBsonOneResult["timestampserver"];
                        if (nowTimestamp - firstTimestamp < compresscycle)
                            return;

                        var lastPointDataBsonOneResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Descending("timestampserver")).FirstOrDefault();
                        var lastTimestamp = (long)lastPointDataBsonOneResult["timestampserver"];
                        pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid) & builderFilter.Lt("timestampserver", lastTimestamp) & builderFilter.Gte("timestampserver", firstTimestamp);
                        var pointDataBsonResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Ascending("timestampserver")).Skip(0).Limit(5000).ToList();
                        if (pointDataBsonResult.Count == 0)
                            return;

                        pointDataBsonResult.ForEach((x) =>
                        {
                            if (x.Contains("_id"))
                                x.Remove("_id");
                        });
                        pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid) & builderFilter.Lte("timestampserver", Convert.ToInt64(pointDataBsonResult[pointDataBsonResult.Count - 1]["timestampserver"])) & builderFilter.Gte("timestampserver", Convert.ToInt64(pointDataBsonResult[0]["timestampserver"]));
                        _context.HistoryPointDatasBson(historycollection).InsertMany(pointDataBsonResult);
                        var deleteResult = _context.RTPointDatasBson(rtcollection).DeleteMany(pointDataFilter);
                        return;
                    }
                    else
                    {
                        var firstPointDataBsonOneResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Ascending("timestampclient")).FirstOrDefault();
                        if (firstPointDataBsonOneResult == null)
                            return;

                        var firstTimestamp = (long)firstPointDataBsonOneResult["timestampclient"];
                        if (nowTimestamp - firstTimestamp < compresscycle)
                            return;

                        var lastPointDataBsonOneResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Descending("timestampclient")).FirstOrDefault();
                        var lastTimestamp = (long)lastPointDataBsonOneResult["timestampclient"];
                        //取值时全部取出，作为压缩算法处理队列数据
                        pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid) & builderFilter.Lte("timestampclient", lastTimestamp) & builderFilter.Gte("timestampclient", firstTimestamp);
                        var pointDataBsonResult = _context.RTPointDatasBson(rtcollection).Find<BsonDocument>(pointDataFilter).Sort(builderSort.Ascending("timestampclient")).Skip(0).Limit(5000).ToList();
                        if (pointDataBsonResult.Count == 0)
                            return;

                        pointDataBsonResult.ForEach((x) =>
                        {
                            if (x.Contains("_id"))
                                x.Remove("_id");
                        });
                        if (iscompress == 0)//不压缩处理
                        {
                            _context.HistoryPointDatasBson(historycollection).InsertMany(pointDataBsonResult);
                            //删除时不留最新数据，不用考虑断电状况,因为没有压缩
                            pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid) & builderFilter.Lte("timestampserver", Convert.ToInt64(pointDataBsonResult[pointDataBsonResult.Count - 1]["timestampserver"])) & builderFilter.Gte("timestampserver", Convert.ToInt64(pointDataBsonResult[0]["timestampserver"]));
                            var deleteResult = _context.RTPointDatasBson(rtcollection).DeleteMany(pointDataFilter);
                        }
                        else//压缩处理
                        {
                            if (pointDataBsonResult.Count <= 1)
                                return;

                            var originData = new List<Point>();
                            pointDataBsonResult.ForEach(x =>
                            {
                                originData.Add(new Point() { pointid = x["pointid"].ToString(), tenantid = x["tenantid"].ToString(), interval = Convert.ToInt32(x["interval"]), quality = Convert.ToDouble(x["quality"]), timestampClient = (long)x["timestampclient"], timestampServer = (long)x["timestampserver"], value = Convert.ToDouble(x["value"]) });
                            });
                            var dataCompressed = new List<Point>();
                            switch (pointtype)
                            {
                                case 2://模拟量
                                    dataCompressed = CompressUtility.CompressMonitorSDT(originData, accuracy);
                                    #region bsondocument demo
                                    //var document = new BsonDocument
                                    //{
                                    //    { "name", "MongoDB" },
                                    //    { "type", "Database" },
                                    //    { "count", 1 },
                                    //    { "info", new BsonDocument
                                    //        {
                                    //            { "x", 203 },
                                    //            { "y", 102 }
                                    //        }
                                    //    }
                                    //};
                                    #endregion
                                    break;
                                case 1://开关量
                                    dataCompressed = CompressUtility.CompressSwitch(originData);
                                    break;
                                case 3://字符串
                                    dataCompressed = CompressUtility.CompressString(originData);
                                    break;
                                default:
                                    break;
                            }
                            var insertBsonDocuments = dataCompressed.Select(i => new BsonDocument{
                                { "pointid",pointid},
                                { "timestampclient",i.timestampClient},
                                { "interval",interval},
                                { "value",i.value },
                                { "quality",i.quality },
                                { "tenantid",tenantid},
                                { "timestampserver",i.timestampServer}
                            });
                            _context.HistoryPointDatasBson(historycollection).InsertMany(insertBsonDocuments);
                            //删除时留一个最新数据，作为下一压缩算法处理队列的第一个数据（用于处理两个处理队列之间发生断电状况）(这里冗余一个数据)
                            pointDataFilter = builderFilter.Eq("pointid", pointid) & builderFilter.Eq("tenantid", tenantid) & builderFilter.Lt("timestampserver", Convert.ToInt64(pointDataBsonResult[pointDataBsonResult.Count - 1]["timestampserver"])) & builderFilter.Gte("timestampserver", Convert.ToInt64(pointDataBsonResult[0]["timestampserver"]));
                            var deleteResult = _context.RTPointDatasBson(rtcollection).DeleteMany(pointDataFilter);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Process compress method:" + ex.Message);
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

        private static bool IsExist(BsonDocument bd, string elementName)
        {
            bool isExist = false;
            IEnumerable<string> strElementNames = bd.Names;
            foreach (string strElementName in strElementNames)
            {
                if (strElementName == elementName)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
    }
}
