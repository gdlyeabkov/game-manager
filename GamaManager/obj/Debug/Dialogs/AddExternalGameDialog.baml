   M S B A M L   `   `   `  ����F  BGamaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:GamaManager.DialogsGamaManager.Dialogs  5   	   /    )GamaManager.Dialogs.AddExternalGameDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:GamaManager.Dialogs     5   	     � Title$#  Добавление игры��5   	   $	��450��5	   	   $	��800��5
   	   .��5      �� .+�5   
   �� $��25 10q�5      $���zВыберите программу, которую хотите добавить в свою библиотеку игр.��5      �� 5   
   .��5      � -   5         �Name$
 apps��5      $	��325��5       � RowDefinitions 5      �� 5       � ColumnDefinitions 5      �� 5      $��25 �5      �� 5      �� 5      .+�5       �� $��0��5!      $��0��5"      �� 5$      $��0��5%      $��1��5&      $��Программы��5'      �� 5)      $��0��5*      $��2��5+      $��Расположение��5,      5.      5/      �� 50   
   $
��0 25q�51       �� Orientation$ 
Horizontal=�52      $��Right=�53      .+�55      �� -   5        $
��25 0q�56      $��75��57      $��25��58      $��
Обзор��5:      �� 5<      -   5         ��Name$ addSelectedAppsBtn��5=      $
��25 0q�5>      $	��175��5?      $��25��5@      $)��#Добавить выбранные��5B      {�.  5C      �� 5E      -   5        $
��25 0q�5F      $��75��5G      $��25��5H      $��Отмена��5J      5L      5M      5N      