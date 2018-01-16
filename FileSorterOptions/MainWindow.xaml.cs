using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media;
using WPFFolderBrowser;

namespace FileSorterOptions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string[] fileTypes;
        private Dictionary<string, TextBox> textBoxes;
        private Dictionary<string, Button> browseButtons;
        private Dictionary<string, ComboBox> comboBoxes;
        private bool deleteMode = false;
       


        public MainWindow()
        {
            
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            if (deleteMode)
                DeleteRow(Int32.Parse((sender as Button).Name.Substring(6, (sender as Button).Name.Length - 6)));
            else { 
                WPFFolderBrowserDialog browserDialog = new WPFFolderBrowserDialog();
                string btnName = (sender as Button).Name;
                string textBoxName = "TextBox" + btnName.Substring(6, btnName.Length - 6);
                if ((bool)browserDialog.ShowDialog())
                    textBoxes[textBoxName].Text = browserDialog.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxes = new Dictionary<string, TextBox>
            {
                ["TextBox1"] = (TextBox)FindName("TextBox1")
            };
            comboBoxes = new Dictionary<string, ComboBox>
            {
                ["ComboBox1"] = (ComboBox)FindName("ComboBox1")
            };
            browseButtons = new Dictionary<string, Button>
            {
                ["Button1"] = (Button)FindName("Button1")
            };
            this.fileTypes = GetSystemFileTypes();
            ComboBoxItem item;
            //File type (extension)
            foreach (string type in fileTypes) {
                item = new ComboBoxItem
                {
                    Content = type
                };
                this.ComboBox1.Items.Add(item);
            }
        }

        private string[] GetSystemFileTypes() {
            var keys = Registry.ClassesRoot.GetSubKeyNames();
            var fileTypes = new List<string>();
            int i = 1;
            while (keys[i].Contains("."))
            {
                fileTypes.Add(keys[i]);
                ++i;
            }
            return fileTypes.ToArray();
        }

        //Add destination directory
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //number used for naming purposes, doest matter if taken from any of stack panels children count
            int number = (ButtonPanel.Children.Count + 1);
            AddTextBox(number);
            AddBrowseButton(number);
            AddComboBox(number);
        }

        private void DeleteRow(int rowNum) {
            TextBoxPanel.Children.Remove(textBoxes["TextBox" + rowNum]);
            ButtonPanel.Children.Remove(browseButtons["Button" + rowNum]);
            SelectPanel.Children.Remove(comboBoxes["ComboBox" + rowNum]);
            textBoxes.Remove("TextBox" + rowNum);
            browseButtons.Remove("Button" + rowNum);
            comboBoxes.Remove("ComboBox" + rowNum);
            this.deleteMode = false;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private void AddTextBox(int num) {
            var textBox = new TextBox
            {
                MinWidth = 180,
                Margin = new Thickness(10, 10, 0, 0),
                IsReadOnly = true,
                MaxLines = 1,
                FontSize = 18,
                TextWrapping = TextWrapping.NoWrap,
                //#FFDEDEDE
                Background = new SolidColorBrush(Color.FromRgb(222, 222, 222)),
                Name = "TextBox" + num
            };
            textBox.PreviewMouseLeftButtonDown += TextBox_Clicked;
            TextBoxPanel.Children.Add(textBox);
            textBoxes["TextBox" + num] = textBox;
        }

        //<Button x:Name="browse" VerticalAlignment="Top" Padding="10, 2" Margin="0, 10, 0, 0" HorizontalContentAlignment="Center" MinWidth="50" VerticalContentAlignment="Center" Click="browse_Click">Browse</Button>
        private void AddBrowseButton(int num) {
            var button = new Button
            {
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(0, 13, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                MinWidth = 50,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = "Browse",
                Name = "Button" + num
            };
            button.Click += new RoutedEventHandler(Browse_Click);
            ButtonPanel.Children.Add(button);
            browseButtons["Button" + num] = button;
        }




        //<ComboBox x:Name="SelectComboBox" HorizontalAlignment="Center" MinHeight="30" Margin="0,10,0,0" VerticalAlignment="Top" Width="100"/>
        private void AddComboBox(int num) {
            var comboBox = new ComboBox
            {
                MinHeight = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 13, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 100,
                ItemsSource = this.fileTypes,
                Name = "ComboBox" + num
            };
            comboBox.PreviewMouseLeftButtonDown += ComboBox_Clicked;
            SelectPanel.Children.Add(comboBox);
            comboBoxes["ComboBox" + num] = comboBox;
        }



        //<TextBox x:Name="DirectoryInput" Margin="10, 10, 0, 0" HorizontalScrollBarVisibility="Auto" IsReadOnly="True" TextWrapping="NoWrap" VerticalAlignment="Top" MinWidth="180" MaxLines="1" FontSize="18.333" HorizontalContentAlignment="Left" HorizontalAlignment="Left" Background="#FFDEDEDE" />


        //Remove destination directory
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
            this.deleteMode = true;
        }

        //Start sorting
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var locations = new Dictionary<string, string>();
            string extension = null;
            string text = null;
            for (int i = 1; i <= textBoxes.Count; ++i) {
                extension = comboBoxes["ComboBox" + i].Text;
                text = textBoxes["TextBox" + i].Text;
                if (string.IsNullOrEmpty(extension) || string.IsNullOrEmpty(text))
                    continue;
                locations[extension] = text;
            }
            if (locations.Count != 0) { 
                Mover mover = new Mover(locations);
                mover.Move();
            }
        }

        //Start sorting service
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (deleteMode)
                DeleteRow(Int32.Parse((sender as TextBox).Name.Substring(7, (sender as TextBox).Name.Length - 7)));
        }

        private void ComboBox_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (deleteMode)
                DeleteRow(Int32.Parse((sender as ComboBox).Name.Substring(8, (sender as ComboBox).Name.Length - 8)));
        }
    }
}
