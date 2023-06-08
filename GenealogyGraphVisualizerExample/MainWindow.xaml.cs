using CsvHelper;
using CsvHelper.Configuration;
using GenealogyGraph;
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace GenealogyGraphVisualizerExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEnumerator<CsvRecord>? iter;
        private string path = "";
        private CsvConfiguration? config;
        private CsvReader? csv;
        private StreamReader? streamReader;

        public MainWindow()
        {
            InitializeComponent();
            var ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.Multiselect = false;
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                    Delimiter = ";"
                };
                path = ofd.FileName;
                streamReader = new StreamReader(path);
                csv = new CsvReader(streamReader, config);
                iter = csv.GetRecords<CsvRecord>().GetEnumerator();
                LoadRecords(iter);
            }
        }

        private int recordIndex = 0;
        private readonly int loadCount = 50;

        private void LoadRecords(IEnumerator<CsvRecord> iter)
        {
            int count = 0;
            while (count < loadCount)
            {
                var lbi = new ListBoxItem();
                if (!iter.MoveNext()) break;
                if (!string.IsNullOrEmpty(iter.Current.relativeName)) continue;
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(new TextBlock() { Text = iter.Current.masterObjectId.ToString(), Margin = new Thickness(5) });
                sp.Children.Add(new TextBlock() { Text = iter.Current.FIO, Margin = new Thickness(5) });
                lbi.Content = sp;
                NameListBox.Items.Add(lbi);
                count++;
                recordIndex++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            if (iter == null) return;
            while (count < loadCount)
            {
                var lbi = new ListBoxItem();                
                if (!iter.MoveNext()) break;
                if (!string.IsNullOrEmpty(iter.Current.relativeName)) continue;
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Children.Add(new TextBlock() { Text = iter.Current.masterObjectId.ToString(), Margin = new Thickness(5)});
                sp.Children.Add(new TextBlock() { Text = iter.Current.FIO, Margin = new Thickness(5) });
                lbi.Content = sp;
                NameListBox.Items.Add(lbi);
                count++;
                recordIndex++;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            csv?.Dispose();
        }

        private void NameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameListBox.SelectedItem == null) return;
            var lbi = NameListBox.SelectedItem as ListBoxItem;
            var sb = lbi?.Content as StackPanel;
            var tb = sb?.Children[0] as TextBlock;
            var masterObjectId = tb?.Text;
            if (masterObjectId == null) return;
            var id = int.Parse(masterObjectId);
            NameListBox.SelectedItem = null;
            csv?.Dispose();

            var treeVisualizer = new TreeVisualizer(path, id, recordIndex);
            treeVisualizer.Closed += (sender, args) => Focus();
            Closed += (sender, args) => treeVisualizer.Close();
            treeVisualizer.Show();
            NameListBox.Items.Clear();
            var textBlock = new TextBlock
            {
                Text = "Ф.И.О.",
                FontWeight = FontWeights.Bold
            };
            NameListBox.Items.Add(textBlock);
            streamReader = new StreamReader(path);
            csv = new CsvReader(streamReader, config);
            iter = csv.GetRecords<CsvRecord>().GetEnumerator();
            LoadRecords(iter);
        }
    }
}
