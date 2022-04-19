using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public void SelectSettingsTabHandler (object sender, MouseEventArgs e)
        {
            StackPanel tab = ((StackPanel)(sender));
            object tabData = tab.DataContext;
            string rawTabIndex = ((string)(tabData));
            int tabIndex = Int32.Parse(rawTabIndex);
            SelectSettingsTab(tabIndex);
        }

        public void SelectSettingsTab (int tabIndex)
        {
            settingsControl.SelectedIndex = tabIndex;
        }

    }
}
