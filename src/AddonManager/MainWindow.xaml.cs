﻿using System.Collections.Generic;
using System.Windows;
using AddonManager.Models;
using System.ComponentModel;
using System;

namespace AddonManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Addon> addons = new List<Addon>();
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public AddonHandler handler = new AddonHandler();

        public MainWindow()
        {
            InitializeComponent();
            prgBar.Visibility = Visibility.Hidden;
            addons = handler.GetAddons();
            UpdateAddonList();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
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
            prgBar.Visibility = Visibility.Visible;
            worker.RunWorkerAsync();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            handler.Dispose();
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
            throw new NotImplementedException();
        }

        private void lstBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnRemove.IsEnabled = true;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            handler.DownloadAddons(addons);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnUpdate.Content = "Update";
            btnUpdate.IsEnabled = true;
            prgBar.Visibility = Visibility.Hidden;
            addons = handler.GetAddons();
            UpdateAddonList();
        }
    }
}
