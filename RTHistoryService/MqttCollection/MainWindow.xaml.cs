using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MqttClient.VoltageViewModel;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace MqttClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _maxVoltage;
        public int MaxVoltage
        {
            get { return _maxVoltage; }
            set { _maxVoltage = value; this.OnPropertyChanged("MaxVoltage"); }
        }

        private int _minVoltage;
        public int MinVoltage
        {
            get { return _minVoltage; }
            set { _minVoltage = value; this.OnPropertyChanged("MinVoltage"); }
        }

        public VoltagePointCollection generalPointCollection;
        public VoltagePointCollection historyPointCollection;
        //DispatcherTimer updateCollectionTimer;

        public MainWindow()
        {
            //var kkkj = NewDate(1536953206336);
            //var kkkj1 = NewDate(1537245622622);
            //var kkkj2 = NewDate(1537245622828);

            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Maximized;
            this.DataContext = this;

            generalPointCollection = new VoltagePointCollection();
            historyPointCollection = new VoltagePointCollection();
            //updateCollectionTimer = new DispatcherTimer();
            //updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(100);
            //updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            //updateCollectionTimer.Start();

            var ds = new EnumerableDataSource<VoltagePoint>(generalPointCollection);
            ds.SetXMapping(x => generaldateAxis.ConvertToDouble(x.Date));
            ds.SetYMapping(y => y.Voltage);
            generalplotter.AddLineGraph(ds, Colors.Green, 2, "Volts");// to use this method you need "using Microsoft.Research.DynamicDataDisplay;"

            MaxVoltage = -2;
            MinVoltage = -2;

            var ds1 = new EnumerableDataSource<VoltagePoint>(historyPointCollection);
            ds1.SetXMapping(x => historydateAxis.ConvertToDouble(x.Date));
            ds1.SetYMapping(y => y.Voltage);
            historyplotter.AddLineGraph(ds1, Colors.Green, 2, "Volts");
        }

        //void updateCollectionTimer_Tick(object sender, EventArgs e)
        //{
        //    i++;
        //    //voltagePointCollection.Add(new VoltagePoint(Math.Sin(i*0.1),DateTime.Now));
        //    //voltagePointCollection.Add(new VoltagePoint(i, DateTime.Now));
        //}

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Task PublishMonitor(IMqttClient mqttClient, string tenantid, string pointid, long timestamp, int interval, double value,int quality)
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

            var sb = new StringBuilder();
            sb.Append("{'pointid':'");
            sb.Append(pointid);
            sb.Append("',");
            sb.Append("'timestampclient':");
            sb.Append(timestamp);
            sb.Append(",");
            sb.Append("'interval':");
            sb.Append(interval);
            sb.Append(",");
            sb.Append("'value':");
            sb.Append(Math.Round(value, 2));
            sb.Append(",");
            sb.Append("'quality':");
            sb.Append(192);
            sb.Append(",");
            sb.Append("'tenantid':'");
            sb.Append(tenantid);
            sb.Append("'");
            sb.Append("}");
            json = sb.ToString();

            //并马上publish一个消息，主题是topic1
            var message = new MqttApplicationMessageBuilder()
            .WithTopic("collection")//表示采集
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
            return mqttClient.PublishAsync(message);
        }

        public Task PublishHeartBeat(IMqttClient mqttClient, string tenantid, string pointid, int interval)
        {
            long timestamp = GetTimestamp(DateTime.Now);
            double value = 1;
            int quality = 192;
            var sb = new StringBuilder();
            sb.Append("{'pointid':'");
            sb.Append(pointid);
            sb.Append("',");
            sb.Append("'timestampclient':");
            sb.Append(timestamp);
            sb.Append(",");
            sb.Append("'interval':");
            sb.Append(interval);
            sb.Append(",");
            sb.Append("'value':");
            sb.Append(Math.Round(value, 2));
            sb.Append(",");
            sb.Append("'quality':");
            sb.Append(quality);
            sb.Append(",");
            sb.Append("'tenantid':'");
            sb.Append(tenantid);
            sb.Append("'");
            sb.Append("}");
            var json = sb.ToString();

            //并马上publish一个消息，主题是topic1
            var message = new MqttApplicationMessageBuilder()
            .WithTopic("collection")//表示采集
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
            return mqttClient.PublishAsync(message);
        }

        public Task PublishSwitch(IMqttClient mqttClient, string tenantid, string pointid, int interval)
        {
            long timestamp = GetTimestamp(DateTime.Now);
            double value =0;
            if (timestamp % 5 != 0)
            {
                value = 1;
            }

            int quality = 192;
            var sb = new StringBuilder();
            sb.Append("{'pointid':'");
            sb.Append(pointid);
            sb.Append("',");
            sb.Append("'timestampclient':");
            sb.Append(timestamp);
            sb.Append(",");
            sb.Append("'interval':");
            sb.Append(interval);
            sb.Append(",");
            sb.Append("'value':");
            sb.Append(Math.Round(value, 2));
            sb.Append(",");
            sb.Append("'quality':");
            sb.Append(quality);
            sb.Append(",");
            sb.Append("'tenantid':'");
            sb.Append(tenantid);
            sb.Append("'");
            sb.Append("}");
            var json = sb.ToString();

            //并马上publish一个消息，主题是topic1
            var message = new MqttApplicationMessageBuilder()
            .WithTopic("collection")//表示采集
            .WithPayload(json)
            .WithAtMostOnceQoS()
            .Build();
            return mqttClient.PublishAsync(message);
        }

        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private void Window_Loaded(object sender, RoutedEventArgs eo)
        {
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
            .WithClientId("dotnetclient1")
            //.WithTcpServer("132.232.98.119", 1883)
            .WithTcpServer("118.24.180.83", 1883)
            .WithCredentials("admin", "public")
            .Build();
            //连接断了后重新连
            mqttClient.Disconnected += async (s, e) =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            };
            mqttClient.ApplicationMessageReceived += (s, e) =>
            {
                //接受到消息并处理
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
                var now = DateTime.Now;
                //System.Diagnostics.Debug.WriteLine(now.ToLocalTime() + "." + now.Millisecond + "--->" + $"Message from dotnet client:+{ Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            };
            mqttClient.Connected += async (s, e) =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
                //连接成功后马上订阅消息,主题是topic3
                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(@"$local/topic3").Build());
                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(@"$queue/topic3").Build());

                Console.WriteLine("### SUBSCRIBED ###");

                Task.Factory.StartNew(() =>
                {
                    double value = 0;
                    do
                    {
                        _manualResetEvent.WaitOne();
                        var nowDatetimestamp = GetTimestamp(DateTime.Now);
                        this.generalplotter.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            generalPointCollection.Add(new VoltagePoint(value, DateTime.Now,200));
                        }));
                        try
                        {
                            PublishMonitor(mqttClient, "0001", "0001", nowDatetimestamp, 200, value, 192).GetAwaiter().GetResult();
                        }
                        catch (Exception exx)
                        {

                        }
                        //Task.Delay(1000).GetAwaiter().GetResult();
                        Thread.Sleep(200);
                        value += 0.5;
                    } while (true);
                });

                //心跳包
                Task.Factory.StartNew(() =>
                {
                    double value = 0;
                    int cycleTimeStamp = 1000;
                    do
                    {
                        try
                        {
                            //PublishHeartBeat(mqttClient, "0001", "0006",200).GetAwaiter().GetResult();
                            //PublishSwitch(mqttClient, "0001", "0002", cycleTimeStamp).GetAwaiter().GetResult();
                        }
                        catch (Exception exx)
                        {

                        }
                        Thread.Sleep(cycleTimeStamp);
                    } while (true);
                });

                ////并马上publish一个消息，主题是topic1
                //var message = new MqttApplicationMessageBuilder()
                //.WithTopic("topic1")
                //.WithPayload("Message from dotnet client")
                //.WithAtMostOnceQoS()
                //.Build();
                //await mqttClient.PublishAsync(message);
            };
            //建立连接
            mqttClient.ConnectAsync(options);
        }

        private bool _setted = false;
        private void btn_general_Click(object sender, RoutedEventArgs e)
        {
            if (!_setted)
            {
                _manualResetEvent.Set();
                _setted = true;
            }
            else
            {
                _manualResetEvent.Reset();
                _setted = false;
            }
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            if ((dateTimePicker2.DateTime - dateTimePicker1.DateTime).TotalHours > 1)
            {
                MessageBox.Show("所选时间不可超过2个小时！");
                return;
            }

            var time1 =GetTimestamp(dateTimePicker1.DateTime);
            var time2 = GetTimestamp(dateTimePicker2.DateTime);
            var result= HTHistorySDK.GetString($"http://jwhdm.com/api/services/app/RTHistoryService/GetDatas?tenantid=0001&pointid={txtPointId.Text}&beginTimestamp={time1}&endtimestamp={time2}");
            var jsonObj = JObject.Parse(result);
            var resultJson = jsonObj["result"];
            if (resultJson == null)
                return;

            var voltagePointCollection = new List<VoltagePoint>();
            foreach (var item in resultJson)
            {
                var value = Convert.ToDouble(item["value"]);
                var datetime = NewDate(Convert.ToInt64(item["timestampclient"]));
                var interval =Convert.ToInt32(item["interval"]);
                if (voltagePointCollection.Count > 0 && voltagePointCollection[voltagePointCollection.Count - 1].Date == datetime)
                    continue;
                voltagePointCollection.Add(new VoltagePoint(value,datetime,interval));
            }

            var valueUnCompressed = new List<VoltagePoint>();
            //var firstItem = voltagePointCollection.First();
            //var endItem= voltagePointCollection.Last();
            int i = 0;
            foreach (var item in voltagePointCollection)
            {
                i++;
                var firstItem = voltagePointCollection.First();
                //var endItem = voltagePointCollection.Last();
                if (valueUnCompressed.Count > 0 && valueUnCompressed[valueUnCompressed.Count - 1].Date == item.Date)
                    continue;
                var currentItemCompressed = UnCompress(voltagePointCollection, dateTimePicker1.DateTime.AddMilliseconds(i * firstItem.Interval));
                if (currentItemCompressed != null && currentItemCompressed.Date >= dateTimePicker1.DateTime && currentItemCompressed.Date <= dateTimePicker2.DateTime)
                    valueUnCompressed.Add(currentItemCompressed);
                if (valueUnCompressed.Count > 300)
                    break;
            }

            this.historyplotter.Dispatcher.BeginInvoke(new Action(delegate
            {
                historyPointCollection.Clear();
                historyPointCollection.AddMany(valueUnCompressed);
            }));
            this.tbMultiLine.Text = result;
        }

        private VoltagePoint UnCompress(IList<VoltagePoint> data, DateTime time)
        {
            var before = data.Where(x => x.Date <=time).FirstOrDefault();
            var after = data.Where(x => x.Date >= time).FirstOrDefault();
            if (before == null || after == null)
                return null;
            var newValue = (after.Voltage - before.Voltage) / (GetTimestamp(after.Date) - GetTimestamp(before.Date)) * (GetTimestamp(time) - GetTimestamp(before.Date)) + before.Voltage;
            return new VoltagePoint(newValue,time,after.Interval);
        }

        /// <summary>
        /// 获取1970-01-01至dateTime的毫秒数
        /// </summary>
        public long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }

        /// <summary>
        /// 根据时间戳timestamp（单位毫秒）计算日期
        /// </summary>
        public DateTime NewDate(long timestamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = dt1970.Ticks + timestamp * 10000;
            return new DateTime(t);
        }
    }
}
