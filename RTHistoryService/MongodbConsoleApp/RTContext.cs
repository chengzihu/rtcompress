using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoTransfer
{
    public class RTContext
    {
        //定义数据库
        private readonly IMongoDatabase _rtdatabase = null;
        private readonly IMongoDatabase _historydatabase = null;
        public RTContext()
        {
            try
            {
                //连接服务器名称 mongo的默认端口27017
                var client = new MongoClient("mongodb://118.24.180.83:27017,132.232.98.119:27017,132.232.99.30:27017");
                if (client != null)
                {
                    //连接数据库
                    _rtdatabase = client.GetDatabase("rt");
                    _historydatabase = client.GetDatabase("history");
                }
            }
            catch (Exception e)
            {

            }
        }

        public IMongoCollection<BsonDocument> RTTenantsBson
        {
            get
            {
                return _rtdatabase.GetCollection<BsonDocument>("tenants");
            }
        }

        public IMongoCollection<BsonDocument> RTPointsBson
        {
            get
            {
                return _rtdatabase.GetCollection<BsonDocument>("points");
            }
        }

        public IMongoCollection<BsonDocument> RTPointDatasBson(string storageCollectionName)
        {
            return _rtdatabase.GetCollection<BsonDocument>(storageCollectionName);
        }

        public IMongoCollection<BsonDocument> HistoryPointDatasBson(string storageCollectionName)
        {
            return _historydatabase.GetCollection<BsonDocument>(storageCollectionName);
        }

        //public IMongoCollection<user> Province
        //{
        //    get
        //    {
        //        return _rtdatabase.GetCollection<user>("user");
        //    }
        //}

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

    public class user
    {
        public ObjectId _id;
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
