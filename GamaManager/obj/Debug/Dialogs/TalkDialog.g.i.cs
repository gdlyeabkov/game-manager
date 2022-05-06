﻿#pragma checksum "..\..\..\Dialogs\TalkDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "834165F35F35E1E52D3969C9300BB0EC0672267194D8710FEA7407354AA0011E"
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
        
        
        #line 68 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl chatControl;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock userIsWritingLabel;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\Dialogs\TalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputChatMsgBox;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\Dialogs\TalkDialog.xaml"
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
            
            #line 56 "..\..\..\Dialogs\TalkDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.InviteFriendsToTalkHandler);
            
            #line default
            #line hidden
            return;
            case 4:
            this.chatControl = ((System.Windows.Controls.TabControl)(target));
            return;
            case 5:
            this.userIsWritingLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.inputChatMsgBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 85 "..\..\..\Dialogs\TalkDialog.xaml"
            this.inputChatMsgBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.InputToChatFieldHandler);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 91 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SendMsgHandler);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 97 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OpenEmojiPopupHandler);
            
            #line default
            #line hidden
            return;
            case 9:
            this.emojiPopup = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 10:
            
            #line 117 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 130 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 143 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.AddEmojiMsgHandler);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 158 "..\..\..\Dialogs\TalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AttachFileHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

