﻿#pragma checksum "..\..\..\Dialogs\LoginDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "2145C3B546D4FCC3D378969F8F0B1BAFA4745A4635C53E0276A67F79AE76F03E"
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
    /// LoginDialog
    /// </summary>
    public partial class LoginDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl links;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox authLoginField;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox authPasswordField;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox registerLoginField;
        
        #line default
        #line hidden
        
        
        #line 127 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox registerPasswordField;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\..\Dialogs\LoginDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox registerConfirmPasswordField;
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/logindialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\LoginDialog.xaml"
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
            
            #line 12 "..\..\..\Dialogs\LoginDialog.xaml"
            ((GamaManager.Dialogs.LoginDialog)(target)).Loaded += new System.Windows.RoutedEventHandler(this.WindowLoadedHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            this.links = ((System.Windows.Controls.TabControl)(target));
            return;
            case 3:
            this.authLoginField = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.authPasswordField = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 5:
            
            #line 75 "..\..\..\Dialogs\LoginDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.LoginHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            this.registerLoginField = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.registerPasswordField = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 8:
            this.registerConfirmPasswordField = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 9:
            
            #line 140 "..\..\..\Dialogs\LoginDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.RegisterHandler);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 153 "..\..\..\Dialogs\LoginDialog.xaml"
            ((System.Windows.Controls.TextBlock)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ToggleModeHandler);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 159 "..\..\..\Dialogs\LoginDialog.xaml"
            ((System.Windows.Controls.TextBlock)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.ToggleModeHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

