   M S B A M L   `   `   `  ����F  BGamaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:GamaManager.DialogsGamaManager.Dialogs  5   	   .    (GamaManager.Dialogs.AddCollectiionDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:GamaManager.Dialogs     5   	     � Title$%  Создать коллецию��5   	   $	��450��5	   	   $	��800��5
   	   
������� � WindowStartupLocation$ CenterScreen=�5   	    � Topmost . .��5      �� $��25q�5   	   .+�5   
   �� $��24(�5      $
��0 15q�5      $'��!Создать коллекцию��5      �� 5   
   $
��0 15q�5      $Q��KВведите название коллекции (обязательно)��5      �� 5   
   -   5         ��Name$ collectionNameBox��5      $��35��5      �� 5   
   $0��*Выберите тип коллекции��5       $
��0 15q�5!      � 5#   
    � RowDefinitions 5$      �� 5%       � ColumnDefinitions 5'      �� 5(      �� 5)      .+�5+      �� -   5        $��0��5,      $��0��5-      .+�50      ]� $��65��51      
�������.+�54      �� $��Center=�55       �� TextAlignment$ Center=�56      $��Обычная��57      59      �� 5:      $����Коллекция - это простой способ навести
порядок в библиотеке. Перетащите
продукты в коллецию или щелкните правой
кнопкой мыши по продукту, чтобы добавить
его в существующую коллекцию.��5;      5=      �� 5>      -   5        $��0��5?      $��1��5@      .+�5C      ]� $��65��5D      
�������.+�5G      �� $��Center=�5H      $ Center=�5I      $��Динамическая��5J      5L      �� 5M      $����Динамические коллекции создаются с
помощью и обновляются по мере
роста вашей библиотеки.��5N      5P      5Q      5R      5S      