   M S B A M L   `   `   `  ����F  BGamaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:GamaManager.DialogsGamaManager.Dialogs  5      *    $GamaManager.Dialogs.InviteTalkDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      3local!clr-namespace:GamaManager.Dialogs     5      _ [MaterialDesignThemes.Wpf, Version=4.4.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � Title$7  1Пригласить в групповой чат��5	      $	��550��5
      $	��800��5      .��5      �� .+�5   
   �� $
��15 5q�5      $��16(�5      $7��1Пригласить в групповой чат��5      �� 5   
    �� Orientation$ 
Horizontal=�5      $
��0 15q�5      .+�5      '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Circle=�5      $
��15 0q�5      �� 5      -   5         ��Name$ talkTitleLabel��5      $#��Название беседы��5      $
��15 0q�5      5       �� 5!   
   $
��15 5q�5"      $3��-Выбрать из списка друзей��5#      �� 5%   
   .��5&      �� -   5         ��Name$ requests��5'      $	��0 5q�5(      $ 
Horizontal=�5)      5+      �� 5,   
   -   5         ��Name$ 	filterBox��5-      $
��15 5q�5.      $
��Left=�5/      $	��500��50      $��30��51      $��Center=�52      .��54   
    � 	Resources 55         hinttext j� 
AlignmentX j� Stretch	 j� TileMode
 !� Visual��%  &	        !� 56      $
 Left=�58      $
 None=�59      $
	 None=�5:      
 5<      ]� 5=      .+�5>      �� $��30��5?   !   $	��500��5@   !   $/��)Начните набирать имя...��5A   !   ���#959595
�������5C   !   5E      5G      5H      .��6!   ��5I      �� 5J       �� 
TargetType" ��6   .�5K      ��  �� Property � �� �� Value$ Center=�6:   �� 5L       � ��$ 12(�61   �� 5M       � ��$
 Auto��6.   �� 5N       � ��$
 Auto��6.    �� Triggers 5O      P� 5P       P� Property � �� P� Value$  ��62   . �5Q      ��  � ��# [  6;   5R      P� 5S       � �� 62   Z� . �5T      ��  � ��# [  6;   5U      5W      .��5X      5Y      �� 5Z   
   .��5[      �� -   5        $ friends��5\      $	��250��5]      5_      �� 5`   
   $
��0 10q�5a      $ 
Horizontal=�5b      .+�5d      �� -   5         ��Name$ inviteTalkBtn��5e      $	��275��5f      $��30��5g      $
��15 0q�5h      {�.  $��Пригласить��5j      5k      �� 5m      -   5        $	��275��5n      $��30��5o      $
��15 0q�5p      $��Закрыть��5q      5r      5t      5u      5v      