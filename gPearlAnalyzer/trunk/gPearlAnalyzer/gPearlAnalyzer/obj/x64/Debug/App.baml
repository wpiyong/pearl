   M S B A M L   `   `   `  ����I  EgPearlAnalyzer, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:gPearlAnalyzer.ModelgPearlAnalyzer.Model  5          gPearlAnalyzer.App   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      2log"clr-namespace:gPearlAnalyzer.Model     5        �� 	Resources  5         ToolbarButton �� 
TargetType �� Triggers P� Property P� Value �� Property �� Value  LogViewerStyle K� IsItemsHost �� DataType#   gPearlAnalyzer.Model.LogEntry	 � IsSharedSizeScope
 � ColumnDefinitions z� SharedSizeGroup �� 
IsReadOnly �� TextWrapping.  (gPearlAnalyzer.Model.CollapsibleLogEntry � RowDefinitions d�Name ��Name �� Triggers P� 
SourceName �� 
TargetName��%�  &	        &	 �     (�� |    5%      *, +)(�� �    54      *, +)7  �� 5   
   " ��6&    5	      P� 5
       � {� .  . �5      ��  �@J�Opacity$ .5Q�60   5      5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      .R�5      �� ��. .��5      �� 5      5      5      �� 5       � �� 5      �� 5      .R�5      $�  . 61   5       5"      5#      �� 5%   
   "  6   .R�5&      � 	 . 
 5'      �� 5(      $
 Date��6'   $
��Auto �6>   �� 5)      .+�5,      �� $��0��66   $
��Bold"�5-      $��5,0,5,0q�6*   ��5,      �� *
DateTime+5-   *   �� 5/      $��1��63    . $��0q�6U   $
 Wrap=�50      ��5/      �� *	Message+50      51      52      �� 54   
   "  6   .R�55      � 	 . 
 56      �� 57      $
 Date��6'   $
��Auto �6>   �� 58       5;      �� 5<      $
��Auto �6$   �� 5=      .+�5@      �� $��0��66   $
��Bold"�5A      $��5,0,5,0q�6*   ��5@      �� *
DateTime+5A   *   �� 5C      $��1��63    . $��0q�6U   $
 Wrap=�5D      ��5C      �� *	Message+5D      d� 5F      $ Expander��6   $��1��61   $��0��6>   $	��Top=�5G      $��+��63   $��Right=�6?   �� 5I      $ Contents��5K      8��  $��1��5J      $��1��6(   $��2��68   $y�	Collapsed=�5K   -   ��5I      �� *
Contents+5K   -   5M       5N      P� 5O      $ Expander��6    �@d�	IsChecked$
 TrueY�6E   . �5P      �� $ Contents��6    � y�$ Visible=�6I   �� 5Q      $ Expander��6    � ��$ -��6F   5R      5T      5W      5X      