   M S B A M L   `   `   `  ����N  JPluggableRepoClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:PluggableRepoClientPluggableRepoClient  5   	   !    PluggableRepoClient.Window1   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:PluggableRepoClient     5   	   -   5          � Title$  Client��5   	   $	��400��6   $	��750��6%   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5       $��40 �6   6*   �� 5!      $��40 �6   6*   �� 5"      $��40 �6   6*   �� 5#      $��30 �6   6*   �� 5$      $��1* �6   6*   �� 5%      $��20 �6   6*   �� 5&      $��50 �6   6*   .+�5(   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   .+�5)      �� -   5        $��no_of_childs��5)      $��11q�6(   $��0q�65   .��6I    Enter Number of Child Builders6i   �� 5*      $��20��6   6!   �� 5+      -   5        $��entered_childs��5+      $��25��6,   $��3q�69   $��70��6E   $��2q�6P   6�   5,      �� 5-   
   $��1��6   $ 
Horizontal=�6"   $��Center=�6;   .+�5.      �� -   5        $��entered_childs_Button��5.      $��20��62   $	��100��6>   .��6n   Start Builder6}   �� 5/      $��40��6   6!   �� 50      -   5        $��exit_childs_Button��50      $��20��6/   {�.  $	��120��6N   .��6{   Stop Pool Processes6�   51      �� 52   
   -   5        $��
repo_files��52      $��2��6   $��10q�6/   $��0q�6<   $��Center=�6P   $��SemiBold"�6m   .��6�   Select files from Repository6�   �� 53   
   $��3��6   $ 
Horizontal=�6"   $��Center=�6;   .+�54      �� -   5        $��test_driver��54      
�������
�������$	��200��6U   $��2q�6a   .��6u   Select a Test Driver6�   �� 55      -   5        $��tested��55      
�������
�������$	��200��6Q   $��2q�6]   .��6q   Select Tested Files6�   �� 56      $	��120��6   6"   �� 57      -	   5        $��test_request��57      
�������
�������$	��200��6W   $��2q�6c   .��6w   Select a Test Request6�   58      �� 59   
   $��4��6   $ 
Horizontal=�6"   $��Center=�6;   .+�5:      �� -
   5        $��testdriver_list��5:   .    �� SelectionMode$ Single=�6   $	��200��6E   5;      �� 5<      -   5        $��tested_list��5<   0   $ Multiple=�6   $	��200��6C   5=      �� 5>      $��10��6   6!   �� 5?      $ Vertical=�6   $��Center=�60   .+�5@      �� -   5        $��buildXml��5@      $	��100��6%   $��20��61   {�.  .��6f   Build Test Request6z   �� 5A      $��20��6   6"   �� 5B      -   5        $��editXml��5B      $	��100��6(   $��20��64   {�.  .��6h   Edit Test Request6{   5C      �� 5D      $��10��6   6!   �� 5E      -   5        $��testrequest_list��5E   .   $ Single=�6   $	��200��6F   5F      5G      �� 5H   
   $��6��6   $ 
Horizontal=�6"   $��10q�6;   $��Center=�6G   .+�5I      �� -   5        $��buildButton��5I      $	��150��6(   {�.  .��6a   Build and Test6q   5J      5K      5L      