   M S B A M L   `   `   `  ����E  AMqttClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:MqttClient
MqttClient  5   	   P LUtility.Tool.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullfGclr-namespace:Utility.Tool.Controls.View;assembly=Utility.Tool.ControlsUtility.Tool.Controls.View 5
   	       MqttClient.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:MqttClient     5   	   Y UDynamicDataDisplay, Version=0.3.0.1, Culture=neutral, PublicKeyToken=5b7d744a7263923f=d34http://research.microsoft.com/DynamicDataDisplay/1.0  5   	   PmyGclr-namespace:Utility.Tool.Controls.View;assembly=Utility.Tool.Controls  5
   	   -   5          � Title$  
MainWindow��5	   	   $	��600��6   $
��1000��6)   .��5      �  � RowDefinitions 5   
   �� 5      6    � ColumnDefinitions 5   
   �� 5      6!   �� 5      6!   �� 5      6!   .+�5   
   �� $��0��6   $��0��6#   .+�5      8  2Microsoft.Research.DynamicDataDisplay.ChartPlotter  -   5          Name$ generalplotter��5      $	��600��66   $	��600��6C      HorizontalAxis 5      I  CMicrosoft.Research.DynamicDataDisplay.Charts.HorizontalDateTimeAxis  5      -   5        $��generaldateAxis��5   0   3  -Microsoft.Research.DynamicDataDisplay.Plotter   Children. 5      2  ,Microsoft.Research.DynamicDataDisplay.Header  $��Georgia)�6   $�� ��61   =  7Microsoft.Research.DynamicDataDisplay.VerticalAxisTitle  5      $��Georgia)�6'   $��生成的数据��6<   ?  9Microsoft.Research.DynamicDataDisplay.HorizontalAxisTitle  5      $��Georgia)�6)   $
��Time��6>   A  ;Microsoft.Research.DynamicDataDisplay.Charts.HorizontalLine  5      
���  ��$��2��6N   =  7Microsoft.Research.DynamicDataDisplay.Charts.SimpleLine   Value 6$   �� *
MaxVoltage+6N     5      
���  ��$��2��6N    6$   �� *
MinVoltage+6N   5      �� 5      -   5         ��Name$ btn_general��5      $��启动暂停��6*   $	��100��6S   6a   5       �� 5!   
   $��0��6   $��1��6#   .+�5"        -   5        $ historyplotter��5"      $	��600��66   $	��600��6C    5#        5$      -   5        $��historydateAxis��5$   0   . 5&        $��Georgia)�6   $�� ��61     5'      $��Georgia)�6'   $��还原的数据��6<     5(      $��Georgia)�6)   $
��Time��6>     5)      
���  ��$��2��6N    6$   �� *
MaxVoltage+6N     5*      
���  ��$��2��6N    6$   �� *
MinVoltage+6N   5+      � 5,      $��	50,10,0,0q�6   .+�5-      �� $��
数据点:��6   6*   �� 5.      -   5         ��Name$ 
txtPointId��5.      $
��0001��6.   $	��100��6:   6H   �� 5/      $��开始时间:��6   6+   /	  )Utility.Tool.Controls.View.DateTimePicker	  50      -   5        $��dateTimePicker1��50   ?   $
��Left=�6$   $	��Top=�6V   $��25��6n   $	��150��6z   �� 51      $��结束时间:��6   6+   	  52      -	   5        $��dateTimePicker2��52   ?   $
��Left=�6$   $	��Top=�6V   $��25��6n   $	��150��6z   �� 53      -
   5        $ 	btn_reset��53      $��启动暂停��6,   $	��100��6S   6a   54      55      �� 56   
   $��0��6   $��2��6#   $��10,20,10,10q�63   .+�57      �� -   5        $��tbMultiLine��58      	 �� TextWrapping$
	 Wrap=�59      
 �� AcceptsReturn
 . " �� VerticalScrollBarVisibility$ Visible=�5;      $	��600��5<      .��5=      5>      5?      5@      5A      