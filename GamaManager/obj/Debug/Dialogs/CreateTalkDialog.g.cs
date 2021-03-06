#pragma checksum "..\..\..\Dialogs\CreateTalkDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "8939950EB1FA9E8F0FDDE841083115155134E1EBF8A561D0AFD43F11DC916754"
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
    /// CreateTalkDialog
    /// </summary>
    public partial class CreateTalkDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 24 "..\..\..\Dialogs\CreateTalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox chatNameBox;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\Dialogs\CreateTalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel requests;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Dialogs\CreateTalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox filterBox;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\Dialogs\CreateTalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel friends;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\Dialogs\CreateTalkDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button createTalkBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/createtalkdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\CreateTalkDialog.xaml"
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
            this.chatNameBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 29 "..\..\..\Dialogs\CreateTalkDialog.xaml"
            this.chatNameBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.DetectChatNameHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            this.requests = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            this.filterBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 50 "..\..\..\Dialogs\CreateTalkDialog.xaml"
            this.filterBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.FilterFriendsHandler);
            
            #line default
            #line hidden
            return;
            case 4:
            this.friends = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 5:
            this.createTalkBtn = ((System.Windows.Controls.Button)(target));
            
            #line 106 "..\..\..\Dialogs\CreateTalkDialog.xaml"
            this.createTalkBtn.Click += new System.Windows.RoutedEventHandler(this.CreateTalkHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 113 "..\..\..\Dialogs\CreateTalkDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

