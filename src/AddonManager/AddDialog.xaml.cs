using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AddonManager
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddDialog : Window
    {
        public AddDialog()
        {
            InitializeComponent();

            List<string> sources = new List<string>();
            sources.Add("Curse");
            sources.Add("ElvUI");
            cbxSources.ItemsSource = sources;
            cbxSources.SelectedItem = "Curse";
        }

        private void cbxSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)cbxSources.SelectedItem == "ElvUI")
            {
                txtName.Text = "ElvUI";
                txtURL.Text = "https://www.tukui.org/download.php?ui=elvui";
                txtURL.IsEnabled = false;
            }
            else
            {
                txtName.Text = String.Empty;
                txtURL.Text = "https://mods.curse.com/addons/wow/";
                txtURL.IsEnabled = true;
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string entry = txtName.Text;
            entry = entry.Replace(' ', '-');
            entry = entry.ToLower();
            txtURL.Text = "https://mods.curse.com/addons/wow/" + entry;
        }

        private void btnAddAddon_Click(object sender, RoutedEventArgs e)
        {
            using (var handler = new AddonHandler())
            {
                string content = handler.AddAddon(txtName.Text, txtURL.Text, (string)cbxSources.SelectedItem);
                if (content == "failed")
                {
                    lblStatus.Content = $"Press Submit to try checking the URL again";
                    MessageBox.Show($"Url for {txtName.Text} is incorrect, click OK to go to curse.com and find the URL manually");
                    System.Diagnostics.Process.Start($"https://mods.curse.com/search?game-slug=wow&search={txtName.Text}");
                }
                else
                {
                    lblStatus.Content = content;
                    btnCancelAdd.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                }
            }
        }

        private void btnCancelAdd_Click(object sender, RoutedEventArgs e)
        {
            var main = this.Owner as MainWindow;
            main.addons = main.handler.GetAddons();
            main.UpdateAddonList();
            Close();
        }
    }
}
