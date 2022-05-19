﻿#pragma checksum "..\..\FriendActivityAsideControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "0C6B420DBD7BAC305CA80621808E23160960357B549DCC5BB4A165507AC019CC"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using GamaManager;
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


namespace GamaManager {
    
    
    /// <summary>
    /// FriendActivityAsideControl
    /// </summary>
    public partial class FriendActivityAsideControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/friendactivityasidecontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\FriendActivityAsideControl.xaml"
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
            
            #line 26 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).KeyUp += new System.Windows.Input.KeyEventHandler(this.DetectFriendSearchHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 122 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).KeyUp += new System.Windows.Input.KeyEventHandler(this.DetectFriendSearchHandler);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 185 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.Image)(target)).ImageFailed += new System.EventHandler<System.Windows.ExceptionRoutedEventArgs>(this.SetDefaultAvatarHandler);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 203 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ReturnToProfileHandler);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 219 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenEditProfileHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 236 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenGameRecommendationsHandler);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 252 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenFriendsSettingsHandler);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 268 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenGroupsSettingsHandler);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 284 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenEquipmentHandler);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 300 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenTradeOffersHandler);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 317 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenContentTabHandler);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 334 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenContentTabHandler);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 351 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenContentTabHandler);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 368 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenContentTabHandler);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 384 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.StackPanel)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenCommentsHistoryHandler);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 417 "..\..\FriendActivityAsideControl.xaml"
            ((System.Windows.Controls.TextBlock)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenAllEventsHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

