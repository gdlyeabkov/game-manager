﻿#pragma checksum "..\..\..\Dialogs\TalkDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "797C1E908E3818D3F9047B2E066AF07ECD7F82B2910AE83626A3AC1BA92B6970"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using GamaManager.Dialogs;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace GamaManager.Dialogs {
    
    
    /// <summary>
    /// TalkDialog
    /// </summary>
    public partial class TalkDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 31 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock talkTitleLabel;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel ownerMenu;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock onlineUsersCountLabel;
        
        #line default
        #line hidden
        
        
        #line 99 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock usersCountLabel;
        
        #line default
        #line hidden
        
        
        #line 133 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel textChannels;
        
        #line default
        #line hidden
        
        
        #line 141 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel voiceChannels;
        
        #line default
        #line hidden
        
        
        #line 145 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel talkAside;
        
        #line default
        #line hidden
        
        
        #line 157 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox usersBox;
        
        #line default
        #line hidden
        
        
        #line 213 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock filteredUsersCountLabel;
        
        #line default
        #line hidden
        
        
        #line 221 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel users;
        
        #line default
        #line hidden
        
        
        #line 225 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl chatControl;
        
        #line default
        #line hidden
        
        
        #line 231 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock userIsWritingLabel;
        
        #line default
        #line hidden
        
        
        #line 240 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup inputChatMsgPopup;
        
        #line default
        #line hidden
        
        
        #line 266 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputChatMsgBox;
        
        #line default
        #line hidden
        
        
        #line 297 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup emojiPopup;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/talkdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\TalkDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 12 "..\..\..\Dialogs\TalkDialog.xaml"
            ((GamaManager.Dialogs.TalkDialog)(target)).Loaded += new System.Windows.RoutedEventHandler(this.InitializeHandler);
            
            #line default
            #line hidden
            
            #line 13 "..\..\..\Dialogs\TalkDialog.xaml"
            ((GamaManager.Dialogs.TalkDialog)(target)).GotFocus += new System.Windows.RoutedEventHandler(this.StopBlinkWindowHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            this.talkTitleLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.ownerMenu = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            
            #line 51 "..\..\..\Dialogs\TalkDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenTalkNotificationsHandler);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 59 "..\..\..\Dialogs\TalkDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.InviteFriendsToTalkHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 67 "..\..\..\Dialogs\TalkDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenTalkSettingsHandler);
            
            #line default
            #line hidden
            return;
            case 7:
            this.onlineUsersCountLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.usersCountLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            
            #line 115 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.TextBlock)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.CreateTextChannelHandler);
            
            #line default
            #line hidden
            return;
            case 10:
            this.textChannels = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 11:
            this.voiceChannels = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 12:
            this.talkAside = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 13:
            
            #line 154 "..\..\..\Dialogs\TalkDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ToggleAsideHandler);
            
            #line default
            #line hidden
            return;
            case 14:
            this.usersBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 162 "..\..\..\Dialogs\TalkDialog.xaml"
            this.usersBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.GetUsersHandler);
            
            #line default
            #line hidden
            return;
            case 15:
            this.filteredUsersCountLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 16:
            this.users = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 17:
            this.chatControl = ((System.Windows.Controls.TabControl)(target));
            return;
            case 18:
            this.userIsWritingLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 19:
            this.inputChatMsgPopup = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 20:
            this.inputChatMsgBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 270 "..\..\..\Dialogs\TalkDialog.xaml"
            this.inputChatMsgBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.InputToChatFieldHandler);
            
            #line default
            #line hidden
            return;
            case 21:
            
            #line 276 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SendMsgHandler);
            
            #line default
            #line hidden
            return;
            case 22:
            
            #line 288 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OpenEmojiPopupHandler);
            
            #line default
            #line hidden
            return;
            case 23:
            this.emojiPopup = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 24:
            
            #line 314 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 25:
            
            #line 327 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 26:
            
            #line 340 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 27:
            
            #line 355 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AttachFileHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

