   M S B A M L   `   `   `  ����I  EgPearlAnalyzer, Version=1.0.1.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:gPearlAnalyzer.ViewModelgPearlAnalyzer.ViewModel  5   	   '    !gPearlAnalyzer.View.ControlNumber   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   5vm&clr-namespace:gPearlAnalyzer.ViewModel     5   	     � Title$  Enter Control Number��5   	    � 
ResizeMode$ NoResize=�6&    � WindowStartupLocation$ CenterScreen=�6<    � SizeToContent$ WidthAndHeight=�5   	    � 	Resources 5         textStyleTextBox �� 
TargetType �� Triggers P� Property P� Value	 �� Property
 �� Value �� RelativeSource �� Path��%�   &	        �� 5	   
   " ��6)    5
      P� 5       �@=�HasError . . �5      �� 	 �@�ToolTip
 5      ��  �� *Self+$) #(Validation.Errors)[0].ErrorContent�5      5      5      .��5      �  � RowDefinitions 5   
   �� 5      6   �� 5      $
��0.2* �6   6,   .+�5   
   �� $��4q�6   ._�5      �� .+�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� .��6   
Control #:6(   �� 5      $	��200��6   #��[  5!      -   'gPearlAnalyzer.ViewModel.FocusExtension   LoadedFocusedElement 5      ��  �� *Self+��5      �� *ControlNumber+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=� �� ValidatesOnDataErrors .  �� NotifyOnValidationError . .��5!   H   5"      5#      5$      5%      �� 5&   
   $��1��6   $ 
Horizontal=�6"   $��Right=�6;   $��5q�6W   .+�5'      �� $��0,0,4,0q�6   $��70��6&    �� 	IsDefault . ��61   �� *	CommandOK+��5(      ��  �� *FindAncestor+ �� AncestorType" �.��6}   OK6�   �� 5)      $��70��6   ��6    �� *CommandCancel+��5*      ��  �� *FindAncestor+" �.��6}   Cancel6�   5+      5,      5-      