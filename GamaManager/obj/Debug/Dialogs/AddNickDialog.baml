   M S B A M L   `   `   `  ����F  BGamaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:GamaManager.DialogsGamaManager.Dialogs  5      '    !GamaManager.Dialogs.AddNickDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      3local!clr-namespace:GamaManager.Dialogs     5        � Title$  AddNickDialog��5      $	��450��6   $	��800��6(   .��5
      �� .+�5   
   �� $��15q�5      $��36(�5      $��Добавить ник��5      �� 5   
   $��15q�5      $i��cЧтобы не потерять друга, назначьте ему постоянный ник.��5      �� 5   
    �� Orientation$ 
Horizontal=�5      .+�5      �� -   5         ��Name$ friendAvatar��5      $��15q�5      $	��100��5      $	��100��5      ��5      �� 5       �� 	UriSource$M Ghttps://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png?�5       5#      �� 5$      .+�5%      �� -   5         ��Name$ friendNameLabel��5&      $��24(�5'      $
��0 10q�5(      $��Имя друга��5)      �� 5+      $��16(�5,      $
��0 10q�5-      $��Текущий ник��5.      �� 50      -   5         ��Name$ friendAliasBox��51      $��24(�52      $	��600��53      $
��0 10q�54      .��55       � 	Resources 56         hinttext j� 
AlignmentX j� Stretch	 j� TileMode
 !� Visual��%�   &	        !� 57      $
 Left=�59      $
 None=�5:      $
	 None=�5;      
 5=      ]� 5>   "   .+�5?   &   �� $��30��5@   )   $	��600��5A   )   $��Введите ник��5B   )   ���#959595
�������5D   )   5F   #   5H      5I      .��6)   ��5J      �� 5K       �� 
TargetType" ��6    .�5L      ��  �� Property � �� �� Value$ Center=�6B   �� 5M       � ��$ 12(�69   �� 5N       � ��$
 Auto��66   �� 5O       � ��$
 Auto��66    �� Triggers 5P   "   P� 5Q   "    P� Property � �� P� Value$  ��6:   . �5R   &   ��  � ��# [  6C   5S   #   P� 5T   "    � �� 6:   Z� . �5U   &   ��  � ��# [  6C   5V   #   5X      .��5Y   %   5Z      5[      5\      �� 5]   
   $ 
Horizontal=�5^      .+�5`      �� -   5        $
��15 0q�5a      $	��275��5b      $��30��5c      $��Подтвердить��5d      5e      �� 5g      -   5        $
��15 0q�5h      $	��275��5i      $��30��5j      $��Отмена��5k      5l      5n      5o      5p      