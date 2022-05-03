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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using C1.WPF;
using C1.WPF.Carousel;

namespace GamaManager
{
    /// <summary>
    /// Логика взаимодействия для CarousalUC.xaml
    /// </summary>
    public partial class CarousalUC : UserControl
    {
        public CarousalUC ()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            for (int i = 1; i <= 5; ++i)
            {
                carouselListBox.Items.Add(new BitmapImage(new Uri(@"C:\Users\ПК\Downloads\default_thumbnail.png")));
            }
        }

    }
}
