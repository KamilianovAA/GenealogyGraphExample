using GenealogyGraph;
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
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Msagl.Drawing;
using System.Threading;
using System.Windows.Media.Converters;
using CsvHelper;

namespace GenealogyGraphVisualizerExample
{
    /// <summary>
    /// Логика взаимодействия для TreeVisualizer.xaml
    /// </summary>
    public partial class TreeVisualizer : Window
    {
        private string path;
        private int id;
        private int recordIndex;
        private WpfGraphBuilder wpfGraphBuilder;
        private AutomaticGraphLayoutControl aglc;
        private CsvGenealogyTree csvReader;

        public TreeVisualizer(string path, int id, int recordIndex)
        {
            InitializeComponent();
            this.id = id;
            this.path = path;
            this.recordIndex = recordIndex;
            csvReader = new CsvGenealogyTree();
            var tree = csvReader.GetPersonRelations(path, id, 6);
            if (tree != null) csvReader.GenerateEdgesToRootEdge(tree);
            //if (tree != null) csvReader.WriteTreeToFile(path, recordIndex);
            wpfGraphBuilder = new WpfGraphBuilder();
            var graph = new Graph();
            if (tree != null) graph = wpfGraphBuilder.VisualizeTree(tree);
            aglc = new AutomaticGraphLayoutControl();
            aglc.Graph = graph;

            aglc.MouseDoubleClick += Aglc_MouseDoubleClick;
            Grid.SetColumn(aglc, 0);
            VisualizationGrid.Children.Add(aglc);
        }

        private void Aglc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dockPanel = aglc?.Content as DockPanel;
            ShowNinetySevenCheck.IsChecked = true;
            OnlyToRootCheck.IsChecked = false;
            OnlyClosestRelations.IsChecked = false;
            var canvas = dockPanel?.Children[0];
            if (canvas == null) return;
            var nodes = aglc?.Graph.Nodes;
            var pos = e.GetPosition(canvas);
            if (nodes == null) return;
            foreach(var node in nodes)
            {                
                var box = node.GeometryNode.BoundingBox;
                if(pos.X >= box.LeftTop.X && pos.Y <= box.LeftTop.Y &&
                    pos.X <= box.RightBottom.X && pos.Y >= box.RightBottom.Y)
                {
                    var csvReader = new CsvGenealogyTree();
                    id = int.Parse(node.Id);
                    var tree = csvReader.GetPersonRelations(path, id, 6);
                    if (tree != null) csvReader.GenerateEdgesToRootEdge(tree);
                    var graph = new Graph();
                    if (tree != null) graph = wpfGraphBuilder.VisualizeTree(tree);
                    if(aglc != null) aglc.Graph = graph;
                }
            }
        }

        private void ShowNinetySevenCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            var graph = aglc.Graph;
            var edges = graph.Edges.Where(x => x.LabelText == "Другая степень родства");
            if (check.IsChecked == true)
            {
                foreach (var edge in edges)
                {
                    edge.IsVisible = true;
                }
            }
            else
            {
                foreach (var edge in edges)
                {
                    edge.IsVisible = false;
                }
            }
        }

        private void OnlyToRootCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            var graph = aglc.Graph;
            var root = graph.Nodes.First();
            foreach (var edge in graph.Edges)
            {
                var data = (Tuple<int, bool>)edge.UserData;
                if (!data.Item2)
                    if (check.IsChecked != null) edge.IsVisible = (bool)!check.IsChecked;
            }
        }

        private void OnlyClosestRelations_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            var graph = aglc.Graph;
            var root = graph.Nodes.First();
            foreach (var edge in graph.Edges)
            {
                var data = (Tuple<int, bool>)edge.UserData;
                if (data.Item1 != 65 && data.Item1 != 56 && data.Item1 != 53 && data.Item1 != 59 &&
                    data.Item1 != 57 && data.Item1 != 58 && data.Item1 != 54 && data.Item1 != 60)
                    if (check.IsChecked != null) edge.IsVisible = (bool)!check.IsChecked;
            }
        }
    }
}
