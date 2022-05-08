﻿#pragma checksum "..\..\..\Dialogs\CreateCategoryDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B1C3DC016F8C920BC82CD2EA5EA161DAFECD6B7FF0EAF9094548FB8B1E0C8A61"
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
    /// CreateCategoryDialog
    /// </summary>
    public partial class CreateCategoryDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox categoryNameBox;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel requests;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox filterBox;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel friends;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button inviteTalkBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/createcategorydialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
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
            this.categoryNameBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.requests = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            this.filterBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 47 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
            this.filterBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.FilterFriendsHandler);
            
            #line default
            #line hidden
            return;
            case 4:
            this.friends = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 5:
            this.inviteTalkBtn = ((System.Windows.Controls.Button)(target));
            
            #line 103 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
            this.inviteTalkBtn.Click += new System.Windows.RoutedEventHandler(this.AddFriendsToCategoryHandler);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 110 "..\..\..\Dialogs\CreateCategoryDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelHandler);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

