using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoTransfer
{
    public class CompressUtility
    {
        /// <summary>
        /// http://www.blogjava.net/oathleo/archive/2011/09/14/358603.html
        /// </summary>
        /// <param name="originData"></param>
        /// <param name="accuracyE"></param>
        /// <returns></returns>
        public static List<Point> CompressMonitorSDT(List<Point> originData, double accuracyE)//后两个参数为其异步编程使用 
        {
            List<Point> dataCompressed = new List<Point>();
            //上门和下门，初始时是关着的
            double upGate = -double.MaxValue;//定义上门
            double downGate = double.MaxValue;//定义下门
            double nowUp, nowDown;//当前数据的上下斜率
            if (originData.Count <= 1)
                return dataCompressed;

            PointContent contentStatus = new PointContent();
            contentStatus.LastReadData = originData[0];//当前数据的前一个数据
            contentStatus.LastStoredData = contentStatus.LastReadData;//最近保存的点
            dataCompressed.Add(contentStatus.LastReadData);
            foreach (var currentPoint in originData)
            {
                contentStatus.CurrentReadData = currentPoint;//当前读取到的数据
                if ((contentStatus.CurrentReadData.timestampClient - contentStatus.LastReadData.timestampClient) > contentStatus.CurrentReadData.interval * 5)
                {
                    contentStatus.LastStoredData = contentStatus.LastReadData;
                    dataCompressed.Add(contentStatus.LastStoredData);
                    dataCompressed.Add(new Point
                    {
                        pointid = contentStatus.LastStoredData.pointid,
                        tenantid = contentStatus.LastStoredData.tenantid,
                        value = contentStatus.LastStoredData.value,
                        quality = 24,
                        interval = contentStatus.LastStoredData.interval,
                        timestampClient = contentStatus.LastStoredData.timestampClient + contentStatus.LastStoredData.interval / 2,
                        timestampServer = contentStatus.LastStoredData.timestampServer + contentStatus.LastStoredData.interval / 2
                    });
                    contentStatus.LastStoredData = contentStatus.CurrentReadData;
                    dataCompressed.Add(contentStatus.LastStoredData);
                    contentStatus.LastReadData = contentStatus.CurrentReadData;
                }
                else
                {
                    nowUp = (currentPoint.value - contentStatus.LastStoredData.value - accuracyE) / (currentPoint.timestampClient - contentStatus.LastStoredData.timestampClient);
                    //判断最大的上斜率,最小的下斜率
                    if (nowUp > upGate)
                        upGate = nowUp;
                    nowDown = (currentPoint.value - contentStatus.LastStoredData.value + accuracyE) / (currentPoint.timestampClient - contentStatus.LastStoredData.timestampClient);
                    if (nowDown < downGate)
                        downGate = nowDown;
                    //不在平行四边内，即内角和大于等于180度
                    if (upGate >= downGate)
                    {
                        contentStatus.LastStoredData = contentStatus.LastReadData;//修改最近保存的点 
                        dataCompressed.Add(contentStatus.LastReadData);//保存前一个点
                        upGate = (currentPoint.value - contentStatus.LastStoredData.value - accuracyE) / (currentPoint.timestampClient - contentStatus.LastStoredData.timestampClient);
                        downGate = (currentPoint.value - contentStatus.LastStoredData.value + accuracyE) / (currentPoint.timestampClient - contentStatus.LastStoredData.timestampClient);
                    }
                    contentStatus.LastReadData = currentPoint;
                }
            }
            //if (dataCompressed.Count == 1)
            //{
            //    dataCompressed.Add(originData[originData.Count - 1]);
            //}
            return dataCompressed;
        }

        /// <summary>
        /// http://www.blogjava.net/oathleo/archive/2011/09/14/358603.html
        /// </summary>
        /// <param name="originData"></param>
        /// <param name="accuracyE"></param>
        /// <returns></returns>
        public static List<Point> SDTCompress(List<Point> originData, double accuracyE)//后两个参数为其异步编程使用 
        {
            List<Point> dataCompressed = new List<Point>();
            //上门和下门，初始时是关着的
            double upGate = -double.MaxValue;//定义上门
            double downGate = double.MaxValue;//定义下门
            double nowUp, nowDown;//当前数据的上下斜率
            if (originData.Count <= 0)
                return null;

            PointContent status = new PointContent();
            status.LastReadData = originData[0];//当前数据的前一个数据
            status.LastStoredData = status.LastReadData;//最近保存的点
            dataCompressed.Add(status.LastReadData); 
            int i = 0;
            foreach (var currentPoint in originData)
            {
                status.CurrentReadData = currentPoint;//当前读取到的数据
                nowUp = (currentPoint.value - status.LastStoredData.value - accuracyE) / (currentPoint.timestampClient - status.LastStoredData.timestampClient);
                //判断最大的上斜率,最小的下斜率
                if (nowUp > upGate)
                    upGate = nowUp;
                nowDown = (currentPoint.value - status.LastStoredData.value + accuracyE) / (currentPoint.timestampClient - status.LastStoredData.timestampClient);
                if (nowDown < downGate)
                    downGate = nowDown;
                //不在平行四边内，即内角和大于等于180度
                if (upGate >= downGate)
                {
                    status.LastStoredData = status.LastReadData;//修改最近保存的点 
                    dataCompressed.Add(status.LastReadData);//保存前一个点
                    upGate = (currentPoint.value - status.LastStoredData.value - accuracyE) / (currentPoint.timestampClient - status.LastStoredData.timestampClient);
                    downGate = (currentPoint.value - status.LastStoredData.value + accuracyE) / (currentPoint.timestampClient - status.LastStoredData.timestampClient);
                }
                status.LastReadData = currentPoint;
                i++;
            }
            
            //if (dataCompressed.Count == 1)
            //{
            //    dataCompressed.Add(originData[originData.Count - 1]);
            //}
            return dataCompressed;
        }

        /// <summary>
        /// http://www.blogjava.net/oathleo/archive/2011/09/14/358603.html
        /// </summary>
        /// <param name="originData"></param>
        /// <returns></returns>
        public static List<Point> CompressSwitch(List<Point> originData)//后两个参数为其异步编程使用 
        {
            var dataCompressed = new List<Point>();
            if (originData.Count <= 0)
                return null;

            PointContent status = new PointContent();
            status.LastReadData = originData[0];//当前数据的前一个数据
            status.LastStoredData = status.LastReadData;//最近保存的点
            dataCompressed.Add(status.LastStoredData);
            for (int i = 0; i < originData.Count(); i++)
            {
                status.CurrentReadData = originData[i];//当前读取到的数据
                if ((status.CurrentReadData.timestampClient - status.LastReadData.timestampClient) > status.CurrentReadData.interval * 5)
                {
                    status.LastStoredData = status.LastReadData;
                    dataCompressed.Add(status.LastStoredData);
                    dataCompressed.Add(new Point
                    {
                        pointid = status.LastStoredData.pointid,
                        tenantid = status.LastStoredData.tenantid,
                        value = status.LastStoredData.value,
                        quality = 24,
                        interval = status.LastStoredData.interval,
                        timestampClient = status.LastStoredData.timestampClient+ status.LastStoredData.interval/2,
                        timestampServer = status.LastStoredData.timestampServer+ status.LastStoredData.interval/2
                    });
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
                else if (status.CurrentReadData.value != status.LastReadData.value)
                {
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
            }

            return dataCompressed;
        }

        /// <summary>
        /// http://www.blogjava.net/oathleo/archive/2011/09/14/358603.html
        /// </summary>
        /// <param name="originData"></param>
        /// <returns></returns>
        public static List<Point> OnlyProgressOffLine(List<Point> originData)//后两个参数为其异步编程使用 
        {
            var dataCompressed = new List<Point>();
            if (originData.Count <= 0)
                return null;

            PointContent status = new PointContent();
            status.LastReadData = originData[0];//当前数据的前一个数据
            status.LastStoredData = status.LastReadData;//最近保存的点
            dataCompressed.Add(status.LastStoredData);
            for (int i = 0; i < originData.Count(); i++)
            {
                status.CurrentReadData = originData[i];//当前读取到的数据
                if ((status.CurrentReadData.timestampClient - status.LastReadData.timestampClient) > status.CurrentReadData.interval * 5)
                {
                    status.LastStoredData = status.LastReadData;
                    dataCompressed.Add(status.LastStoredData);
                    dataCompressed.Add(new Point
                    {
                        pointid = status.LastStoredData.pointid,
                        tenantid = status.LastStoredData.tenantid,
                        value = status.LastStoredData.value,
                        quality = 24,
                        interval = status.LastStoredData.interval,
                        timestampClient = status.LastStoredData.timestampClient + status.LastStoredData.interval / 2,
                        timestampServer = status.LastStoredData.timestampServer + status.LastStoredData.interval / 2
                    });
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
                else
                {
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
            }

            return dataCompressed;
        }

        /// <summary>
        /// http://www.blogjava.net/oathleo/archive/2011/09/14/358603.html
        /// </summary>
        /// <param name="originData"></param>
        /// <returns></returns>
        public static List<Point> CompressString(List<Point> originData)//后两个参数为其异步编程使用 
        {
            var dataCompressed = new List<Point>();
            if (originData.Count <= 0)
                return null;

            PointContent status = new PointContent();
            status.LastReadData = originData[0];//当前数据的前一个数据
            status.LastStoredData = status.LastReadData;//最近保存的点
            dataCompressed.Add(status.LastStoredData);
            for (int i = 0; i < originData.Count(); i++)
            {
                status.CurrentReadData = originData[i];//当前读取到的数据
                if ((status.CurrentReadData.timestampClient - status.LastReadData.timestampClient) > status.CurrentReadData.interval * 5)
                {
                    status.LastStoredData = status.LastReadData;
                    dataCompressed.Add(status.LastStoredData);
                    dataCompressed.Add(new Point
                    {
                        pointid = status.LastStoredData.pointid,
                        tenantid = status.LastStoredData.tenantid,
                        value = status.LastStoredData.value,
                        quality = 24,
                        interval = status.LastStoredData.interval,
                        timestampClient = status.LastStoredData.timestampClient + status.LastStoredData.interval / 2,
                        timestampServer = status.LastStoredData.timestampServer + status.LastStoredData.interval / 2
                    });
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
                else if (status.CurrentReadData.value.Equals(status.LastReadData.value))
                {
                    status.LastStoredData = status.CurrentReadData;
                    dataCompressed.Add(status.LastStoredData);
                    status.LastReadData = status.CurrentReadData;
                }
            }

            return dataCompressed;
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

        public class PointContent
        {
            public Point CurrentReadData { get; set; }//当前读取数据，当前读取到的数据

            public Point LastReadData { get; set; }//上一个读取数据，当前数据的前一个数据

            public Point LastStoredData { get; set; }//上一个保存数据,最近保存的点 
        }

        public class Point
        {
            //public Point(string pointid,string tenantid,long timestampClient, long timestampServer,double value,double quality,int interval)
            //{
            //    this.pointid = pointid;
            //    this.tenantid = tenantid;
            //    this.timestampClient = timestampClient;
            //    this.timestampServer = timestampServer;
            //    this.value = value;
            //    this.quality = quality;
            //    this.interval = interval;
            //}

            public Point()
            {
            }

            public string pointid { get; set; }
            public string tenantid { get; set; }
            public long timestampClient { get; set; }
            public long timestampServer { get; set; }
            public double value { get; set; }
            public double quality { get; set; }
            public int interval { get; set; }
        }

        /// <summary> 
        /// 线性插值解压 
        /// </summary> 
        /// <param name="originData">原始数据</param> 
        /// <param name="SDTData">压缩数据</param>
        /// <param name="SDTData">目标值横坐标</param>
        /// <returns></returns> 
        //public List<Point> GetSingleSDTUncompress(IList<Point> SDTData,long n)
        //{
        //    List<Point> UncompreeDate = new List<Point>();
        //    Point newPoint = new Point();
        //    if (SDTData.Count <= 1)
        //        return null;

        //    var  before=SDTData.Where(data=>data.timestampClient<=n).LastOrDefault();
        //    var after = SDTData.Where(data => data.timestampClient >= n).FirstOrDefault();
        //    if (before.timestampClient == after.timestampClient)
        //    {
        //        UncompreeDate.Add(before);
        //        return UncompreeDate;
        //    }

        //    double k = (after.value - (before.value)) / (after.timestampClient - before.timestampClient);
        //    newPoint.timestampClient =n;
        //    newPoint.value = k * (n - before.timestampClient)
        //    + before.value;
        //    UncompreeDate.Add(newPoint);
        //    return UncompreeDate;
        //}

        ///// <summary> 
        ///// 线性插值解压 
        ///// </summary> 
        ///// <param name="originData">原始数据</param> 
        ///// <param name="SDTData">压缩数据</param>
        ///// <returns></returns> 
        //public List<Point> SDTUncompress(List<Point> originData, IList<Point> SDTData)
        //{
        //    List<Point> UncompreeDate = new List<Point>();
        //    Point newPoint = new Point();
        //    int num = 0;
        //    if (SDTData.Count <= 1)
        //        return null;
        //    for (int i = 0; i < SDTData.Count - 1; i++)
        //    {
        //        double k = (SDTData[i + 1].Y - (SDTData[i].Y)) / (SDTData[i + 1].X - (SDTData[i].X));
        //        int startIndex = FindIndex(originData, SDTData[i].X);
        //        int endIndex = FindIndex(originData, SDTData[i + 1].X);
        //        for (int j = startIndex; j < endIndex; j++)
        //        {
        //            newPoint.X = originData[j].X;
        //            newPoint.Y = k * (originData[j].X - SDTData[i].X)
        //            + SDTData[i].Y;
        //            UncompreeDate.Add(newPoint);
        //            num++;
        //        }
        //    }
        //    if (UncompreeDate.Count < originData.Count)
        //    {
        //        int startIndex1 = FindIndex(originData, SDTData.LastOrDefault().X);
        //        for (int j = startIndex1; j < originData.Count; j++)
        //        {
        //            newPoint.X = originData[j].X;
        //            double k = (SDTData[SDTData.Count - 1].Y - (SDTData[SDTData.Count - 2].Y)) / (SDTData[SDTData.Count - 1].X - (SDTData[SDTData.Count - 2].X));
        //            newPoint.Y = k * (originData[j].X - SDTData[SDTData.Count - 1].X)
        //            + SDTData[SDTData.Count - 1].Y;
        //            UncompreeDate.Add(newPoint);
        //            num++;
        //        }
        //    }

        //    return UncompreeDate;
        //}
        //private int FindIndex(List<Point> list, double value)
        //{
        //    int index = list.FindIndex(s => s.X == value);
        //    return index;
        //}
    }
}
