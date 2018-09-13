using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmqMongoConnect
{
    public class RTContext
    {
        //定义数据库
        private readonly IMongoDatabase _database = null;
        public RTContext()
        {
            try
            {
                //连接服务器名称 mongo的默认端口27017
                var client = new MongoClient("mongodb://118.24.180.83:27017,132.232.98.119:27017,132.232.99.30:27017");
                if (client != null)
                    //连接数据库
                    _database = client.GetDatabase("rt");
            }
            catch (Exception e)
            {

            }
        }

        public IMongoDatabase Database
        {
            get
            {
                return _database;
            }
        }

        public IMongoCollection<BsonDocument> ProvinceBson
        {
            get
            {
                return _database.GetCollection<BsonDocument>("user");
            }
        }


        public IMongoCollection<user> Province
        {
            get
            {
                return _database.GetCollection<user>("user");
            }
        }
    }

    public class user
    {
        public ObjectId _id;
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
