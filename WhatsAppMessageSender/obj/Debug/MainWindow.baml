   M S B A M L   `   `   `  ����P  LWhatsAppMessageSender, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WhatsAppMessageWhatsAppMessage  5   	   T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls 5   	        WhatsAppMessage.MainWindow   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:WhatsAppMessage     5   	   KMah;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5   	   (  "MahApps.Metro.Controls.MetroWindow    TitleCharacterCasing$  Normal=�5	   	    � Title$" WhatsAppMessageSender ver0.1��5
   	   $	��460��6.   $	��530��6;      	GlowBrush
 ��  �$��0q�5   	    � 
ResizeMode$ NoResize=�5   	    � 	Resources 5       �� Source�� 5   
   $ /Style/StyleDic.xaml?�6   %    .��5      �  5   
      proxyElement��%   &	        � 5      6   5      .+�5   
   �� $y�	Collapsed=�6   #��[  60   �� 5   
   -   5         ��Name$ rec_overlay��5      $	��530��6)   $	��330��65     rec-overlay#��[ 6B   )  #MahApps.Metro.Controls.ProgressRing  5   
   -   5          Name$ loading_image��5        
pgr-normal#��[ 62   �� 5   
   -   5         ��Name$ read_button��5      $��Read CSV��6&   $
��Left=�69   $��18,8,0,0q�6T   $	��Top=�6f   $��76��6~   $��30��6�    ' #MahApps.Styles.Button.Square.Accent#���  6�   +  %MahApps.Metro.Controls.ControlsHelper	   ContentCharacterCasing$	 Normal=�6�   &   System.Windows.Controls.DataGrid  5   
   -   5        $��dataGrid��5      $��Center=�6#   $��18,38,10,208q�6@   $	��496��6V   
   AutoGenerateColumns
 .     CanUserAddRows .     
IsReadOnly .    CanUserResizeColumns .    casing-normal   ColumnHeaderStyle# [ 5      ��5   b   ��  �� Path$ PhoneNumbers�   Columns 5      4  .System.Windows.Controls.DataGridTemplateColumn  5       ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .    Width5  /System.Windows.Controls.DataGridLengthConverter$ 43 6;      Header 5!      �� 5"       d� 	IsChecked 6#   �� $ 
IsCheckAll� � DataContext 5#   $   ��  �� RelativeSource ��  �� AncestorType"  $ DataContext�{�5$   #   �� $ EnableSelecAll�   CellTemplate 5&      �� 5'      .R�5(      �� $��Center=�5*   )   $��Center=�6D    5(   '   �� $ 	IsChecked� �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�5*   D   5+      5-      0  *System.Windows.Controls.DataGridTextColumn  5.      $ NO��5/   %    . 1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 5.   %   �� *IndexNumber+ 	  ClipboardContentBinding 6E   Z� 5/   1     50      $
 NAME��51   %    .  50   %   �� *CustomerName+ 6F   Z� 51   3     52      $ 	TELEPHONE��53   %    .  52   %   �� *PhoneNumber+ 6E   Z� 53   8     54      $ EMAIL��55   %    .  54   %   �� *Email+ 6?   Z� 55   4     56      $ PENERIMA��57   %    .  56   %   �� *
Penerima+ 6B   Z� 57   7     58      $ 
KETERANGAN��59   %    .  58   %   �� *
Keteramgam+ 6D   Z� 59   9     5:      $  ��5;   %    .  5:   %   �� *Judge+ 6?   Z� 5;   /   5=      �� 5?   
   -   5         ��Name$ WhatsAppMessage��5?       �� AcceptsReturn .  �� TextWrapping$
 Wrap=�6@   *
  $MahApps.Metro.Controls.TextBoxHelper  
  	Watermark$  Write here the message��5@      "! �� VerticalScrollBarVisibility$
! Auto=�5A      $	��Top=�5B      $��19,220,8.667,48q�61   �� 5C   
   -   5        $ send_button��5C      $��SEND MESSAGE��6&   $
��Left=�6=   $��
66,391,0,0q�6X   $	��Top=�6l   $	��400��5D      $��30��6   #���  6C   $	 Normal=�6�   5E      5F      