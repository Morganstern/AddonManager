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
        }

        private void cbxSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)cbxSources.SelectedItem == "ElvUI")
            {
                txtName.Text = "ElvUI";
                txtURL.Text = "ElvUI";
            }
            else
            {
                txtName.Text = String.Empty;
                txtURL.Text = String.Empty;
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string entry = txtName.Text;
            entry = entry.Replace(' ', '-');
            entry = entry.ToLower();
            txtURL.Text = entry;
        }

        private void btnAddAddon_Click(object sender, RoutedEventArgs e)
        {
            if(AddonHandler.AddAddon(txtName.Text, txtURL.Text))
            {
                lblStatus.Content = $"{txtName.Text} added";
            }
        }

        private void btnCancelAdd_Click(object sender, RoutedEventArgs e)
        {
            var main = this.Owner as MainWindow;
            main.UpdateAddonList();
            Close();
        }
    }
}
