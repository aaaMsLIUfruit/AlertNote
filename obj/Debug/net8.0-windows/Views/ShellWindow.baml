   M S B A M L   `   `   `  ����G  CStickyAlerts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:StickyAlerts.ViewsStickyAlerts.Views  5      A%clr-namespace:StickyAlerts.ViewModelsStickyAlerts.ViewModels  5	      $    StickyAlerts.Views.ShellWindow   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      ,local clr-namespace:StickyAlerts.Views  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      6
viewmodels%clr-namespace:StickyAlerts.ViewModels  5	      $	��350��5
      $	��500��5      $	��300��5      (  "HandyControl.Controls.WindowAttach    HideWhenClosing$
  True��5       � ShowActivated$
 True��5      "  HandyControl.Controls.Window   ShowNonClientArea$ False��5         	ShowTitle$ False��5       � WindowStartupLocation$ CenterScreen=�5       � WindowStyle$ SingleBorderWindow=�5       � 	Resources 5         ControlButtonStyle  ButtonCustom �� BasedOn �� 
TargetType3  -HandyControl.Controls.BackgroundSwitchElement	 �� Property  SecondaryRegionBrush
 �� Value  PrimaryTextBrush  CloseButtonStyle��%@  &	        7  &	 �    7   �� 5   
   8   " ��5      .�5      �� 	 �@ MouseHoverBackground#
 �  6P   �� 5      	 �@ MouseDownBackground#
 �  6O   �� 5      	 � ��$
 45��6&   �� 5      	 � ��$
 35��6'   �� 5      	 � ��$
 18(�6)   �� 5      	 � ��$
 Segoe Fluent Icons)�6+   �� 5       	 � ��

 ���� 6+   �� 5!      	 � ��#
 �  6+   5"      �� 5#   
   8   " ��5&      .�5'      �� 	 �@ MouseHoverBackground

 �  ��6P   �� 5(      	 �@ MouseDownBackground

 �  ��6O   5)      5*      .��5+      �  � RowDefinitions 5,   
   �� 5-      $
��Auto �6   �� 5.      $��* �6   .+�52   
   � $��0��53         IsDragElement$
 True��54        RegionBrush#���  55      .+�57      7  1HandyControl.Controls.TransitioningContentControl  $
��Left=�58         TransitionMode$
 Fade=�59      y�5:      �� *IsSettingsVisible+ ! Boolean2VisibilityReConverter �� 	Converter# [ .��5;      ��  �� Orientation$ 
Horizontal=�6   .+�5<      �� -   5         ��Name$ AddMenuButton��5<      $	����5=      #��[  5>       � ContextMenu 5@      �� 5A      .@�5B   "   m� $��添加新便签��6+    m� Command 6:   �� *AddAlertCommand+m� 5C   "   -   5        $��导入作业为便签��5C   +   6<   5D      5F      �� 5G      $	����5I      #���   5J      ��5H      �� *SwitchToSettingsViewCommand+5J      �� 5K      -   5        $	����5K      #��[  6O   $��5,0,0,0q�6{   5L      5M        5O      $
��Left=�5P      $
 Fade=�5Q      y�5R      �� *IsSettingsVisible+  Boolean2VisibilityConverter# [ .��5S      �� $	����5U      #��[  5V      ��5T      �� *SwitchToAlertsViewCommand+5V      5W      �� 5Y      $��Center=�5Z      $��Center=�5[      
������ $��16(�5]       J� IsHitTestVisible$ False��5^      $�� ��5_      �� 5a      $��Right=�6   $ 
Horizontal=�65   .+�5c      �� $	����5e      #���   5f      ��5d      �� *MinimizeCommand+5f      �� 5h      $	����5j      #���  5k      ��5i      �� *ExitCommand+5k      5l      5m        5p   
   $��1��5q      $
 Fade=�5r      y�5s      �� *IsSettingsVisible+# [ .��5t      #   StickyAlerts.Views.AlertsView   � DataContext 6   �� *AlertsViewModel+5u        5v   
   $��1��5w      $
 Fade=�5x      y�5y      �� *IsSettingsVisible+# [ .��5z      %   StickyAlerts.Views.SettingsView   6!   �� *SettingsViewModel+5{      &   HandyControl.Controls.NotifyIcon  5~   
   -   5           Icon$Q Kpack://application:,,,/StickyAlerts;component/Resources/Images/Calendar.ico��5         Text$ StickyAlerts��5�      y�5�      �� *IsNotifyIconVisible+# [  5�      �� 5�      .@�5�      m� -   5        $��Center=�5�      $��显示主页面��5�       m� Icon 5�      �� 5�      $��Center=�5�   !   $��Center=�5�   !   $��Segoe Fluent Icons)�5�   !   $��14(�5�   !   $	����5�   !   5�      m� 5�      -   5        $��Center=�5�      $��退出��5�       5�      �� 5�      $��Center=�5�   !   $��Center=�5�   !   $��Segoe Fluent Icons)�5�   !   $��14(�5�   !   $	����5�   !   5�      5�      5�      5�      5�      