#pragma checksum "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "70EC5EFB4CCC6FEEF70F150A3302CF7422DD2BBD6CC0DEDB46C5F808783C4CEC"
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
    /// ScreenShotsUploaderDialog
    /// </summary>
    public partial class ScreenShotsUploaderDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 37 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel actionBtns;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox screenShotsFilter;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.WrapPanel screenShots;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl screenShotsControl;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image mainScreenShot;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock mainScreenShotDateLabel;
        
        #line default
        #line hidden
        
        
        #line 141 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock mainScreenShotSizeLabel;
        
        #line default
        #line hidden
        
        
        #line 148 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox descBox;
        
        #line default
        #line hidden
        
        
        #line 193 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox spoilerCheckBox;
        
        #line default
        #line hidden
        
        
        #line 197 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock charsLeftLabel;
        
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
            System.Uri resourceLocater = new System.Uri("/GamaManager;component/dialogs/screenshotsuploaderdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
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
            
            #line 11 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
            ((GamaManager.Dialogs.ScreenShotsUploaderDialog)(target)).Loaded += new System.Windows.RoutedEventHandler(this.AppLoadedHandler);
            
            #line default
            #line hidden
            return;
            case 2:
            this.actionBtns = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            
            #line 53 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.UploadScreenShotHandler);
            
            #line default
            #line hidden
            return;
            case 4:
            this.screenShotsFilter = ((System.Windows.Controls.ComboBox)(target));
            
            #line 73 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
            this.screenShotsFilter.DropDownClosed += new System.EventHandler(this.SelectScreenShotsFilterHandler);
            
            #line default
            #line hidden
            return;
            case 5:
            this.screenShots = ((System.Windows.Controls.WrapPanel)(target));
            return;
            case 6:
            this.screenShotsControl = ((System.Windows.Controls.TabControl)(target));
            return;
            case 7:
            this.mainScreenShot = ((System.Windows.Controls.Image)(target));
            return;
            case 8:
            this.mainScreenShotDateLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.mainScreenShotSizeLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 10:
            this.descBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 152 "..\..\..\Dialogs\ScreenShotsUploaderDialog.xaml"
            this.descBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.DetectDescChangedHandler);
            
            #line default
            #line hidden
            return;
            case 11:
            this.spoilerCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 12:
            this.charsLeftLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

