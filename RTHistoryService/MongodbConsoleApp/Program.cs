using MongoDB.Bson;
using MongoDB.Driver;
using MongoTransfer;
using System;
using System.Collections.Generic;

namespace MongodbConsoleApp
{
    class Program
    {
        private static readonly RTContext _context = new RTContext();
        static void Main(string[] args)
        {
            var begin = RTContext.GetTimestamp(DateTime.Now.AddDays(-2));
            var end = RTContext.GetTimestamp(DateTime.Now);
            var result = GetBsonDocuments("0001","0001",begin,end);
            //1536907984449 1537080784451
            Console.WriteLine(result);
            Console.Read();
        }

        public static string GetBsonDocuments(string tenantid,string pointid,long beginTimestamp,long endTimestamp)
        {
            //创建约束生成器
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            //FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "jack36");
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("pointid", $"{pointid}") & builderFilter.Eq("tenantid", $"{pointid}");
            filter &=builderFilter.Gte("timestampclient",beginTimestamp);
            filter &= builderFilter.Lte("timestampclient", endTimestamp);
            //获取数据
            var tenantResult = _context.HistoryPointDatasBson($"{tenantid}.datas").Find<BsonDocument>(filter).Sort(Builders<BsonDocument>.Sort.Ascending("timestampclient")).Skip(0).Limit(2000);
            return tenantResult.ToList().ToJson();
        }

        public static string GetBsonDocumentsLte(string tenantid, string pointid,long endTimestamp)
        {
            //创建约束生成器
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            //FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "jack36");
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("pointid", $"{pointid}") & builderFilter.Eq("tenantid", $"{pointid}");
            filter &= builderFilter.Lte("timestampclient", endTimestamp);
            //获取数据
            var tenantResult = _context.HistoryPointDatasBson($"{tenantid}.datas").Find<BsonDocument>(filter).Sort(Builders<BsonDocument>.Sort.Ascending("timestampclient")).Skip(0).Limit(2000);
            return tenantResult.ToList().ToJson(); 
        }

        public static string GetBsonDocumentsGte(string tenantid, string pointid, long beginTimestamp)
        {
            //创建约束生成器
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            //约束条件
            //FilterDefinition<BsonDocument> filter = builderFilter.Eq("name", "jack36");
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("pointid", $"{pointid}") & builderFilter.Eq("tenantid", $"{pointid}");
            filter &= builderFilter.Gte("timestampclient", beginTimestamp);
            //获取数据
            var tenantResult = _context.HistoryPointDatasBson($"{tenantid}.datas").Find<BsonDocument>(filter).Sort(Builders<BsonDocument>.Sort.Ascending("timestampclient")).Skip(0).Limit(2000);
            return tenantResult.ToList().ToJson();
        }

    }
}
