   M S B A M L   `   `   `  ����G  CStickyAlerts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:StickyAlerts.ViewsStickyAlerts.Views  5      9!clr-namespace:StickyAlerts.ModelsStickyAlerts.Models  5	      A%clr-namespace:StickyAlerts.ViewModelsStickyAlerts.ViewModels  5
      $    StickyAlerts.Views.AlertWindow   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      ,local clr-namespace:StickyAlerts.Views  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      .models!clr-namespace:StickyAlerts.Models  5	      6
viewmodels%clr-namespace:StickyAlerts.ViewModels  5
        � Title$  AlertWindow��5      $	��300��5      (  "HandyControl.Controls.WindowAttach   IgnoreAltF4$
 True��5         ShowInTaskManager$ False��5         RegionBrush#���   5      $��1q�5       � 
ResizeMode$ NoResize=�5       � ShowInTaskbar$ False��5      "  HandyControl.Controls.Window   ShowNonClientArea$ False��5       � SizeToContent$ Height=�5       � Left 5      �� *Left+ �� Mode$ OneWay=�
	 � Top	 5      �� *Top+$ OneWay=�
 � Topmost
 5      �� *	Topmost+y�5      �� *AlertVisible+$ TwoWay=�  Boolean2VisibilityConverter �� 	Converter# [ .��5      �    IsDragElement$
 True��6   
������  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       $
��Auto �6    � ContextMenu 5"   
   �� 5#      $	��200��6   .@�5$      m� $��打开便笺列表��5&        HomeIconTextBlock m� Icon# [ 5'       m� Command 5%      �� *ShowShellCommand+5'      m� 5(      $��删除便笺��5*        DeleteIconTextBlock# [ 5+       5)      �� *DeleteCommand+5+      m� 5,      $��对齐便笺��5.        AlginIconTextBlock# [ 5/       5-      �� *AlginCommand+5/      �� 50        SeparatorBaseStyle#���  6   m� 51      $��窗体置顶��52       m� IsCheckable$
 True��53       m� 	IsChecked 54      �� *	Topmost+m� 55      $��便笺可见��56        HideIconTextBlock# [ 57      $
 True��58       59      �� *AlertVisible+m� 5:      $��注释可见��5;      $
 True��5<       5=      �� *NoteVisible+m� 5>      $��倒计时可见��5?      $
 True��5@       5A      �� *CountdownVisible+�� 5B      #���  6   m� 5C      $��退出程序��5E        ExitIconTextBlock# [ 5F       5D      �� *ExitCommand+5F      5G      .+�5J   
   �� $��10��6     PrimaryBrush#���  6   #  HandyControl.Controls.TextBox  5M   
   $��1��5N      $��10,10,10,10q�5O      $��0q�5P      $��0q�5Q      $��20(�5S      $
��Bold"�5T       	 PrimaryTextBrush#��� 	 5U       �� TextWrapping$
 Wrap=�5W       5R      Z� ��5V      �� *Title+ �� UpdateSourceTrigger$ PropertyChanged=�5W      �� 5Z   
   $��2��5[      $��10,-10,10,0q�5\       �� Orientation$ 
Horizontal=�5]      y�5^      �� *CountdownVisible+# [ .+�5_      �� $��Bottom=�5`      $��50(�5a      #���  5b      ��5c      �� *Remaining.Days+$ OneWay=��� 5d      $��5,0,5,10q�5e      $��Bottom=�5f      $��20(�5g      $
��Bold"�5h       
 SecondaryTextBrush#��� 
 5i      $	��天��5j      �� 5k      $��Bottom=�5l      $��50(�5m      #���  5n      ��5o      �� *Remaining.Hours+$ OneWay=��� 5p      $��5,0,5,10q�5q      $��Bottom=�5r      $��20(�5s      $
��Bold"�5t      #��� 
 5u      $	��时��5v      �� 5w      $��Bottom=�5x      $��50(�5y      #���  5z      ��5{      �� *Remaining.Minutes+$ OneWay=��� 5|      $��5,0,5,10q�5}      $��Bottom=�5~      $��20(�5      $
��Bold"�5�      #��� 
 5�      $	��分��5�      5�      �� 5�   
   $��3��5�        BorderBrush#���  5�      $��0,1,0,0q�5�      y�5�      �� *NoteVisible+# [ ._�5�        $��10,10,10,10q�5�       �� AcceptsReturn$
 True��5�      $��0q�5�      #��� 
 5�        TextBoxExtend#���  5�      $
 Wrap=�5�      ��5�      �� *Note+$ PropertyChanged=�5�      5�      5�      5�      