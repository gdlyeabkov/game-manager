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
    /// Логика взаимодействия для AcceptExitDialog.xaml
    /// </summary>
    public partial class AcceptExitDialog : Window
    {
        public AcceptExitDialog()
        {
            InitializeComponent();

            Initialize();
        
        }

        public void Initialize ()
        {
            this.DataContext = null;
        }


        private void CloseClientHandler (object sender, RoutedEventArgs e)
        {
            CloseClient();
        }

        public void CloseClient ()
        {
            this.DataContext = "OK";
            Cancel();
        }

        private void CancelHandler (object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            object data = this.DataContext;
            bool isDataNotSetted = data == null;
            if (isDataNotSetted)
            {
                this.DataContext = "Cancel";
            }
            this.Close();
        }

    }
}
