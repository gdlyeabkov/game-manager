   M S B A M L   `   `   `  ����F  BGamaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:GamaManager.DialogsGamaManager.Dialogs  5      ?$clr-namespace:GamaManager.PropertiesGamaManager.Properties  5	      '    !GamaManager.Dialogs.FriendsDialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      3local!clr-namespace:GamaManager.Dialogs     5      _ [MaterialDesignThemes.Wpf, Version=4.4.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      ,p$clr-namespace:GamaManager.Properties  5	      -   5        &    GamaManager.Properties.Resources    friendsLabelContent � Title# Z  5
      $	��675��5      $	��450��5       � Topmost . .��5      ]� .+�5   
   ��  �� Orientation$ 
Horizontal=�5      $	��Top=�5      .+�5      �� -   5         ��Name$ userProfileAvatar��5      $��15q�5      $��50��5      $��50��5      $��Center=�5      ��5      �� 5       �� 	UriSource$M Ghttps://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png?�5      5"      �� 5#      $��Center=�5$      .+�5&      �� $ 
Horizontal=�5'      .+�5)      �� -   5         ��Name$ userProfileNameLabel��5*      $
��15 0q�5+      $��16(�5,      $%��Имя пользователя��5-      $��Center=�5.      '  !MaterialDesignThemes.Wpf.PackIcon  50      -   5        $��24��51      $��24��52      $
��15 0q�53         Kind$ ChevronDown=�54      $��Center=�55      56      58      �� 59      -   5        $ userProfileStatusLabel��5:      $
��15 0q�5;      $+��%Статус пользователя��5<      5>        5?      -   5        $��24��5@      $��24��5A      $ Settings=�5B      $��Bottom=�5C      5D      � 5F      -   5         �Name$ userProfilePopup��5G      	 � 	StaysOpen	 .  $	��450��5I      $	��275��5J      $��Mouse=�5K      ��5M      �� 5N      
�������.+�5Q      �� -   5        $ onlineStatusLabel��5R      $��14(�5S      $��15 10q�5T      $��В сети��5U      
 � DataContext$
 online��5V      5W      �� 5Y      $��14(�5Z      $��15 10q�5[      $��Нет на месте��5\      �� 5^      $
��15 0q�5_      .+�5a      �� -	   5        $ invisibleStatusLabel��5b   !   $��14(�5c   !   $��Невидимка��5d   !   �� 5f      
�������$	��0 5q�5h   !   $����Вы будуте показываться как не в сети, но по-прежнему
сможете пользоваться чатом. друзей и чатов��5i   !   5k      �� 5l      $
��15 0q�5m      .+�5o      �� -
   5        $ offlineStatusLabel��5p   !   $��14(�5q   !   $��Не в сети��5r   !   $
 offline��5s   !   5t   !   �� 5v      
�������$	��0 5q�5x   !   $?��9Выйти из системы друзей и чатов��5y   !   5{      �� 5|      �� 5}      $ 
Horizontal=�5~      .+�5�      �� $��Center=�5�   !   $
��15 0q�5�   !   $��Не беспокоить��5�   !   �� 5�      $��Center=�5�   !   $��14(�5�   !   $��15 10q�5�   !   $D��>Отключает все уведомления в чатах��5�   !   5�      �� 5�      �� 5�      -   5        $��14(�5�      $��15 10q�5�      $,��&Изменить имя профиля��5�      5�      � 5�      -   5        $ updateProfilePopup��5�      $	��500��5�      $	��250��5�      $��Mouse=�5�      ��5�      �� 5�   "   
�������.+�5�   &   �� $��24(�5�   )   �� 5�   &   $g��aДругие пользователи видят ваши аватар и имя профиляю��5�   )   �� 5�   &   $ 
Horizontal=�5�   )   .+�5�   *   �� -   5        $ updateProfilePopupAvatar��5�   -   $��15q�5�   -   $��50��5�   -   $��50��5�   -   ��5�   .   �� 5�   2   $M Ghttps://cdn2.iconfinder.com/data/icons/ios-7-icons/50/user_male-128.png?�5�   5   5�   +   �� 5�   *   $
��15 0q�5�   -   .+�5�   .   �� $	��0 5q�5�   1   $1��+Изменить имя профиля на��5�   1   �� 5�   .   -   5         ��Name$ updateProfilePopupNameBox��5�   1   $	��0 5q�5�   1   $	��275��5�   1   $��30��5�   1   5�   +   5�   '   �� 5�   &   $ 
Horizontal=�5�   )   .+�5�   *   �� -   5        $
��15 0q�5�   -   $��30��5�   -   $	��125��5�   -   $��Подтвердить��5�   -   5�   -   �� 5�   *   -   5        $
��15 0q�5�   -   $��30��5�   -   $	��125��5�   -   $��Отмена��5�   -   5�   -   5�   '   5�   #   5�      5�      5�      5�      �� 5�   
   �� 5�   
   -   5         ��Name$ favoriteFriendsWrap��5�      $��Visible=�5�      $��Disabled=�5�      $	��Top=�5�      $	��100��5�      .��5�      �� -   5         ��Name$ favoriteFriends��5�      $ 
Horizontal=�5�      5�      �� 5�   
   .+�5�      �� $ 
Horizontal=�5�      $��35��5�      .+�5�      �� $
��10 5q�5�      $��Center=�5�      #��Z  5�      �� 5�      -   5        $ keywordsLabel��5�      $
��10 5q�5�      $	��275��5�      5�        5�      -   5        $	 Add=�5�      $
��10 5q�5�      $��Center=�5�      5�      5�      �� 5�      $	��350��5      .��5     �� .+�5     �� -   5        $ 
categories��5     �� 5     -   5        $ friends��5     �� 5	     -   5        $ customPlayedCategories��5
     �� 5     -   5        $ onlineFriendsCountLabel��5     $��14(�5     $"��Друзья в сети (0)��5     $��15q�5     �� 5      -   5        $ onlineFriends��5!     �� 5#     �� 5&     -   5        $ offlineFriendsCountLabel��5'     $��Не в сети (0)��5(     $��14(�5)     $��15q�5*     �� 5,     -   5        $ offlineFriends��5-     5/     50     �� 51     $ 
Horizontal=�52     $
��0 15q�53     $��35��54     .+�56     �� $��Center=�57     $
��15 0q�58     $!��Групповые чаты��59       5;     -   5        $��Center=�5<     $
��15 0q�5=     $	 Add=�5>     $��24��5?     $��24��5@     5A     5C     �� 5D     -   5        $ 	talksWrap��5E     $y�	Collapsed=�5F     $��85��5G     .��5I     �� -   5        $ talks��5J     5L       5M     $��Right=�5N     $��32��5O     $��32��5P     $
 Drag=�5Q     5S     5T     5U     