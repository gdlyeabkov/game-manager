using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
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

            Initialize();
        
        }

        public void Initialize()
        {
            GetOffers();
        }

        public void GetOffers ()
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://loud-reminiscent-jackrabbit.glitch.me/api/games/get");
                webRequest.Method = "GET";
                webRequest.UserAgent = ".NET Framework Test Client";
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        GamesListResponseInfo myobj = (GamesListResponseInfo)js.Deserialize(objText, typeof(GamesListResponseInfo));
                        string responseStatus = myobj.status;
                        bool isOKStatus = responseStatus == "OK";
                        if (isOKStatus)
                        {
                            List<GameResponseInfo> loadedGames = myobj.games;
                            int countLoadedGames = loadedGames.Count;
                            bool isGamesExists = countLoadedGames >= 1;
                            if (isGamesExists)
                            {
                                foreach (GameResponseInfo gamesListItem in myobj.games)
                                {
                                    TabItem newGame = new TabItem();
                                    string gamesListItemName = gamesListItem.name;
                                    string gamesListItemUrl = @"https://loud-reminiscent-jackrabbit.glitch.me/api/game/distributive/?name=" + gamesListItemName;
                                    string gamesListItemImage = @"https://loud-reminiscent-jackrabbit.glitch.me/api/game/thumbnail/?name=" + gamesListItemName;
                                    Image newGamePhoto = new Image();
                                    newGamePhoto.Margin = new Thickness(5);
                                    newGamePhoto.Width = 500;
                                    newGamePhoto.Height = 500;
                                    newGamePhoto.BeginInit();
                                    Uri newGamePhotoUri = new Uri(gamesListItemImage);
                                    newGamePhoto.Source = new BitmapImage(newGamePhotoUri);
                                    newGamePhoto.EndInit();
                                    newGame.Content = newGamePhoto;
                                    offersControl.Items.Add(newGame);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
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
