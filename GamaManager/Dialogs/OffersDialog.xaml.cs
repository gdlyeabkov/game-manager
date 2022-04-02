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
    /// Логика взаимодействия для OffersDialog.xaml
    /// </summary>
    public partial class OffersDialog : Window
    {
        public OffersDialog()
        {
            InitializeComponent();
        }

        private void BackBtnHandler(object sender, RoutedEventArgs e)
        {
            nextBtn.IsEnabled = true;
            ItemCollection offers = offersControl.Items;
            int countOffers = offers.Count;
            int selectedOfferIndex = offersControl.SelectedIndex;
            bool isCanChangeOffer = selectedOfferIndex > 0;
            if (isCanChangeOffer)
            {
                offersControl.SelectedIndex = selectedOfferIndex - 1;
                selectedOfferIndex = offersControl.SelectedIndex;
                bool isCanNotChangeOffer = selectedOfferIndex <= 0;
                if (isCanNotChangeOffer)
                {
                    backBtn.IsEnabled = false;
                }
            }
        }

        private void NextBtnHandler(object sender, RoutedEventArgs e)
        {
            backBtn.IsEnabled = true;
            ItemCollection offers = offersControl.Items;
            int countOffers = offers.Count;
            int selectedOfferIndex = offersControl.SelectedIndex;
            bool isCanChangeOffer = selectedOfferIndex < countOffers - 1;
            if (isCanChangeOffer)
            {
                offersControl.SelectedIndex = selectedOfferIndex + 1;
                selectedOfferIndex = offersControl.SelectedIndex;
                bool isCanNotChangeOffer = selectedOfferIndex >= countOffers - 1;
                if (isCanNotChangeOffer)
                {
                    nextBtn.IsEnabled = false;
                }
            }
        }

        private void CancelBtnHandler(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
