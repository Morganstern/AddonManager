using System.Collections.Generic;
using System.Windows;
using AddonManager.Models;

namespace AddonManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Addon> addons = new List<Addon>(AddonHandler.GetAddons());

        public MainWindow()
        {
            InitializeComponent();
            UpdateAddonList();
        }

        public void UpdateAddonList()
        {
            List<string> addonList = new List<string>();
            foreach (var a in addons)
                addonList.Add(a.Name);
            addonList.Sort();
            lstBox.ItemsSource = addonList;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.Content = "Updating";
            btnUpdate.IsEnabled = false;

            AddonHandler.DownloadAddons(addons);

            btnUpdate.Content = "Update";
            btnUpdate.IsEnabled = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddDialog window = new AddDialog();
            window.Owner = this;
            window.ShowDialog();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lstBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnRemove.IsEnabled = true;
        }
    }
}
