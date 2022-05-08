using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Логика взаимодействия для AddCollectiionDialog.xaml
    /// </summary>
    public partial class AddCollectiionDialog : Window
    {

        public string currentUserId = "";

        public AddCollectiionDialog(string currentUserId)
        {
            InitializeComponent();

            Initialize(currentUserId);

        }

        public void Initialize (string currentUserId)
        {
            this.currentUserId = currentUserId;
        }

        private void CreateSimpleCollectionHandler (object sender, MouseButtonEventArgs e)
        {
            CreateSimpleCollection();
        }

        public void CreateSimpleCollection ()
        {
            Environment.SpecialFolder localApplicationDataFolder = Environment.SpecialFolder.LocalApplicationData;
            string localApplicationDataFolderPath = Environment.GetFolderPath(localApplicationDataFolder);
            string saveDataFilePath = localApplicationDataFolderPath + @"\OfficeWare\GameManager\" + currentUserId + @"\save-data.txt";
            JavaScriptSerializer js = new JavaScriptSerializer();
            string saveDataFileContent = File.ReadAllText(saveDataFilePath);
            SavedContent loadedContent = js.Deserialize<SavedContent>(saveDataFileContent);
            List<Game> currentGames = loadedContent.games;
            List<FriendSettings> currentFriends = loadedContent.friends;
            Settings currentSettings = loadedContent.settings;
            List<string> updatedCollections = loadedContent.collections;
            Notifications currentNotifications = loadedContent.notifications;
            List<string> currentCategories = loadedContent.categories;
            string collectionNameBoxContent = collectionNameBox.Text;
            updatedCollections.Add(collectionNameBoxContent);
            string savedContent = js.Serialize(new SavedContent
            {
                games = currentGames,
                friends = currentFriends,
                settings = currentSettings,
                collections = updatedCollections,
                notifications = currentNotifications,
                categories = currentCategories
            });
            File.WriteAllText(saveDataFilePath, savedContent);
            this.Close();
        }

        private void CreateDynamicCollectionHandler (object sender, MouseButtonEventArgs e)
        {
            CreateDynamicCollection();
        }

        public void CreateDynamicCollection ()
        {
            this.Close();
        }

    }
}
