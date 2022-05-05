﻿#pragma checksum "..\..\..\Dialogs\FriendsDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "6235D37C8CA167496BAB24943FA5A7FF8CD44ABFDD7AEC35AD77A531E8BC15AA"
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
using GamaManager.Properties;
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
    /// FriendsDialog
    /// </summary>
    public partial class FriendsDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Dialogs\FriendsDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer favoriteFriendsWrap;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Dialogs\FriendsDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel favoriteFriends;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\Dialogs\FriendsDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox keywordsLabel;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\Dialogs\FriendsDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel friends;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\..\Dialogs\FriendsDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel talks;
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/friendsdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\FriendsDialog.xaml"
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
            
            #line 13 "..\..\..\Dialogs\FriendsDialog.xaml"
            ((GamaManager.Dialogs.FriendsDialog)(target)).Loaded += new System.Windows.RoutedEventHandler(this.InitSocketsHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            this.favoriteFriendsWrap = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 3:
            this.favoriteFriends = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.keywordsLabel = ((System.Windows.Controls.TextBox)(target));
            
            #line 44 "..\..\..\Dialogs\FriendsDialog.xaml"
            this.keywordsLabel.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.FilterFriendsHandler);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 50 "..\..\..\Dialogs\FriendsDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenAddFriendDialogHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            this.friends = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 7:
            
            #line 76 "..\..\..\Dialogs\FriendsDialog.xaml"
            ((MaterialDesignThemes.Wpf.PackIcon)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.OpenCreateTalkDialogHandler);
            
            #line default
            #line hidden
            return;
            case 8:
            this.talks = ((System.Windows.Controls.StackPanel)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

