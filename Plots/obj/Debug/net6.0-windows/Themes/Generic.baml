   M S B A M L   `   `   `  ����@  <Plots, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:InteractiveDataDisplay.WPFInteractiveDataDisplay.WPF  5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;    5InteractiveDataDisplay.WPF.CountToVisibilityConverter   CountToVisibilityConverter)   #InteractiveDataDisplay.WPF.PlotAxis  �� 
TargetType �� Property �� Value �� 
TargetType%   InteractiveDataDisplay.WPF.Axis  Name   AxisOrientation �� Mode �� RelativeSource   
IsReversed	   Ticks
   
Foreground&    InteractiveDataDisplay.WPF.Chart'   !InteractiveDataDisplay.WPF.Figure  Name   BorderThickness   ExtraPadding)   #InteractiveDataDisplay.WPF.PlotBase   
PlotHeight   	PlotWidth   PlotOriginX   PlotOriginY   IsXAxisReversed   IsYAxisReversed   IsAutoFitEnabled   AspectRatio0   *InteractiveDataDisplay.WPF.MouseNavigation  Name"   IsVerticalNavigationEnabled"   IsVerticalNavigationEnabled$   IsHorizontalNavigationEnabled$   IsHorizontalNavigationEnabled3   -InteractiveDataDisplay.WPF.KeyboardNavigation  Name"   IsVerticalNavigationEnabled$   IsHorizontalNavigationEnabled7   1InteractiveDataDisplay.WPF.VerticalContentControl   	Placement   	LeftTitle   Name!   AxisOrientation"   
IsReversed)	   #InteractiveDataDisplay.WPF.AxisGrid# 	 Name$ 	  Stroke% J� Opacity& 	  VerticalTicks' �� ElementName( 	  HorizontalTicks) 	  IsXAxisReversed* 	  IsYAxisReversed+   Title,   BottomTitle-   
RightTitle'
   !InteractiveDataDisplay.WPF.Legend. 
 Name/   LegendContent0   LegendVisibility1 ��Name2 �� RadiusX3 �� RadiusY4 J� IsHitTestVisible5 �� 	Converter6 ��Name7 ��Name�� 5      F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     5      /d3(clr-namespace:InteractiveDataDisplay.WPF  5      %�  &	        '       '  %    '
  E    7   '  �       5      6$   �� 5
      "   6   .�5   
   ��  � �� 5      �� 5      "  6"   .R�5      � .+�5        $ 	PART_Axis��6"    5   "   �� *AxisOrientation+$ TwoWay=� �� *TemplatedParent+ 5   "   �� *
IsReversed+$ TwoWay=� �� *TemplatedParent+	 5   "   �� *Ticks+$ TwoWay=� �� *TemplatedParent+
 5   "   �� *
Foreground+$ TwoWay=� �� *TemplatedParent+�� 5      5      5      5      �� 5   
    � ��$ False��6&   5      �� 5      "   6   .�5   
   ��  � �� 5      �� 5      "  6"   .R�5       � .+�5!        $ PART_figure��6$   $��1q�69   # z 5*   $   #��z��5+   $    5"   $   �� *
PlotHeight+$ TwoWay=� �� *TemplatedParent+ 5#   $   �� *	PlotWidth+$ TwoWay=� �� *TemplatedParent+ 5$   $   �� *PlotOriginX+$ TwoWay=� �� *TemplatedParent+ 5%   $   �� *PlotOriginY+$ TwoWay=� �� *TemplatedParent+ 5&   $   �� *IsXAxisReversed+$ TwoWay=� �� *TemplatedParent+ 5'   $   �� *IsXAxisReversed+$ TwoWay=� �� *TemplatedParent+ 5(   $   �� *IsAutoFitEnabled+$ TwoWay=� �� *TemplatedParent+ 5)   $   �� *AspectRatio+$ TwoWay=� �� *TemplatedParent+.+�5,        $ PART_mouseNavigation��5.   1   # z 5,   1   # z 5-   1   5.   1     5/      $ PART_keyboardNavigation��51   4   # z 5/   4   # z 50   4   51   4     52      $
 Left=�68   #��z 53   8   $��Center=�54   8   $��False��55   8     56      $  PART_verticalAxis��6*   $
 Left=�57   *   $
! Left=�58   *   #��z��5:   *   " 59   *   �� *IsYAxisReversed+$ TwoWay=� �� *TemplatedParent+.��5;   "     $ False��65   5<      	  5=      $# PART_axisGrid��6*   #$ z��5B   *   $
% 0.25Q�6P   & 5>   *   �� *Ticks+$' PART_verticalAxis��$ OneWay=�( 5?   *   �� *Ticks+$' PART_horizontalAxis��$ OneWay=�) 5@   *   �� *IsXAxisReversed+$ TwoWay=� �� *TemplatedParent+* 5A   *   �� *IsXAxisReversed+$ TwoWay=� �� *TemplatedParent+5B   P   �� 5C      $	 Top=�6-   $��Center=�5D   -   $��16(�5E   -   #��z+ 5F   -   #��z��5G   -   $��False��5H   -   �� 5I      $ Bottom=�6-   $��Center=�5J   -   #��z, 5K   -   #��z��5L   -   $��False��5M   -     5N      $ Right=�68   #��z- 5O   8   $��Center=�5P   8   $��False��5Q   8     5R      $  PART_horizontalAxis��6*   $ Bottom=�5S   *   $! Bottom=�5T   *   #��z��5V   *   " 5U   *   �� *IsXAxisReversed+$ TwoWay=� �� *TemplatedParent+.��5W   "     $ False��65   5X      �� 5Y      �� 5Z      #��z 6%   #��z��5[   %   $ Center=�6P   
  5\      $. PART_legend��6(   
���   �#��z/ 5]   ;   #y�z0 5^   (   5_      �� 5`      $1 FocusVisualElement��6$   $2 2��6@   $3 2��6L   ���	#FF6DBDD1$��1��6k   $% 0Q�6   $4 false��6�   5a      5b      5d      �� 5e   
    � ��$ False��6&   5f      �� 5h      "  
 6   .�5i   
   ��  � �� 5j      �� 5k      " 
 6"   .R�5l      � 
������ $��10,10,10,10q�64   $	��Top=�6I   $��Right=�5m      .+�5n      �� $1 legendBorder��6$   
���   �$��Stretch=�6I   $��Stretch=�6e   $2 10��5o      $3 10��6*   
�������$
% 0.75Q�6D   y�5p      �� *
ItemsCount+ �� *TemplatedParent+85   �� 5q      $6 scrollViewer��6'   $��10,10,10,10q�5r   '   $
��Auto=�6<   $
��Auto=�5s   '   $��False��6L   $��0q�5t   '   .��5u      �� 5v      5w      5x      5z      5{      �� 5}      "   6   .�5~   
   ��  � �� 5      �� 5�      "  6"   .R�5�      �� $7 	Presenter��5�      #��z��5�      #��z��5�      5�      5�      5�      5�      