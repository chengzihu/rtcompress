using MqttClient.VoltageViewModel;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        public VoltagePointCollection voltagePointCollection;
        DispatcherTimer updateCollectionTimer;
        private int i = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            voltagePointCollection = new VoltagePointCollection();
            updateCollectionTimer = new DispatcherTimer();
            updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(100);
            //updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
            //updateCollectionTimer.Start();

            var ds = new EnumerableDataSource<VoltagePoint>(voltagePointCollection);
            ds.SetXMapping(x => dateAxis.ConvertToDouble(x.Date));
            ds.SetYMapping(y => y.Voltage);
            plotter.AddLineGraph(ds, Colors.Green, 2, "Volts");// to use this method you need "using Microsoft.Research.DynamicDataDisplay;"

            MaxVoltage = 1;
            MinVoltage = -1;
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
            sb.Append(1);
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
                System.Diagnostics.Debug.WriteLine(now.ToLocalTime() + "." + now.Millisecond + "--->" + $"Message from dotnet client:+{ Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
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
                        var nowDatetimestamp = GetTimestamp(DateTime.Now);
                        //for (int i = 0; i < 10000; i++)
                        //{
                        this.plotter.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            voltagePointCollection.Add(new VoltagePoint(value, DateTime.Now));
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
                        //}
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
    }
}
