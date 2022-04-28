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
    /// Логика взаимодействия для ActivationGameDialog.xaml
    /// </summary>
    public partial class ActivationGameDialog : Window
    {
        public ActivationGameDialog()
        {
            InitializeComponent();
        }

        private void NextHandler(object sender, RoutedEventArgs e)
        {
            Next();
        }

        public void Next()
        {
            int index = activationControl.SelectedIndex;
            activationControl.SelectedIndex = index + 1;
        }

        private void BackHandler(object sender, RoutedEventArgs e)
        {
            Back();
        }

        private void Back()
        {
            int index = activationControl.SelectedIndex;
            activationControl.SelectedIndex = index - 1;
        }

        private void CancelHandler(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public void Cancel()
        {
            this.Close();
        }


        private void AcceptHandler(object sender, RoutedEventArgs e)
        {
            Accept();
        }

        public void Accept()
        {
            Cancel();
        }

    }

}