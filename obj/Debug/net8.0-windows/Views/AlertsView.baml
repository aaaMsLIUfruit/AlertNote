   M S B A M L   `   `   `  ����G  CStickyAlerts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:StickyAlerts.ViewsStickyAlerts.Views  5      +clr-namespace:StickyAlertsStickyAlerts  5      A%clr-namespace:StickyAlerts.ViewModelsStickyAlerts.ViewModels  5
      #    StickyAlerts.Views.AlertsView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      ,local clr-namespace:StickyAlerts.Views  5      %coreclr-namespace:StickyAlerts  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5	      6
viewmodels%clr-namespace:StickyAlerts.ViewModels  5
        � 	Resources  5         AlertListTemplate �� DataType,   &StickyAlerts.ViewModels.AlertViewModel   HandyControl.Controls.Card,  &HandyControl.Controls.SimpleStackPanel  UnderlineTextBoxStyle �� UpdateSourceTrigger �� Orientation �� 
IsEditable   StickyAlerts.ThemeManager   	ThemeList+   %StickyAlerts.TextBoxMaxLengthBehavior   	MaxLength �� Text � ColumnDefinitions*  $HandyControl.Controls.DateTimePicker   TertiarySmokeBackgroundBrush	 � Cursor  TertiaryForegroundBrush
   SelectedDateTime  ToggleButtonSwitch.Small d� 	IsChecked  ButtonDanger.Small �� RelativeSource �� AncestorType��%�  &	        �� 5   
   "  61   .R�5        $��	AlertCard��6   $��5,5,5,0q�6'   .��5        .+�5      �� $��5q�5      $��Center=�5      $��Right=�5      #���  5      ��5      �� *Title+$ PropertyChanged=��� 5      $ 
Horizontal=�6!   $��5,0,0,0q�6:   $��Center=�6K   .+�5      �� $��主题��6$   $��Center=�6.   $��0,0,5,0q�6I   �� 5      -   5        $	��120��5      $
 True��5       #��Z 5!      $ 5��5#       5"      �� *Theme+$ PropertyChanged=�5%      �� 5&      -   5        $��添加备注��5&   !   $��5,0,0,0q�60   6A   5'      � 5*      $��2��6    5+      �� 5,      $
��Auto �6/   �� 5-      $��* �6/   �� 5.      $
��Auto �6/   .+�50        $��30��51      $��5q�52      #���  53      $
	 Hand��54      #���  55      
 56      �� *
Deadline+�� 58      $��2��6%   $ 
Horizontal=�65   .+�59      d� $��5,0,5,0q�5:   !   $��展示��5;   !   $
	 Hand��5<   !   #���  5>   !    5=   !   �� *AlertVisible+5>   !   �� 5?      $��30��5@   !   $��20��5A   !   $��5,0,0,0q�5B   !   $��0q�5C   !   $	����5F   !   $
	 Hand��5G   !   $��Segoe Fluent Icons)�5H   !   #���  5I   !   ��5D   !   �� *DataContext.DeleteCommand+ �� *FindAncestor+ UserControl��5E   !   �� *Id+5I   !   5J      5K      5L      5M      5N      5O      .��5P      � .+�5Q      ��   BorderBrush#���  6   $��0,1,0,0q�69   ._�5R   
   (  "HandyControl.Controls.ScrollViewer  .��5S        .+�5T      9� )  #HandyControl.Controls.BorderElement   CornerRadius$ 0��5U      $��0,0,0,1q�5V      $
	 Hand��5W       9� 
IsExpanded$
 True��5X      ��5Y      �� 5Z      ��6$   �� *ActivedAlerts.Count+ �� StringFormat$  进行中（共 {0} 个）��.��5\      �� $��0q�5]      #���   5^      $��False��5`      $
��Auto=�5a      ��5_      �� *ActivedAlerts+5a      5b      9� 5c      $ 0��5d      $��0,0,0,1q�5e      $
	 Hand��5f      $ False��5g      ��5h      �� 5i      ��6$   �� *UnactivedAlerts.Count+$  已到期（共 {0} 个）��.��5k      �� $��0q�5l      #���   5m      $��False��5o      $
��Auto=�5p      ��5n      �� *UnactivedAlerts+5p      5q      5r      5s      5t      5u      5v      