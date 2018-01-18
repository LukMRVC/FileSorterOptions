using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using WPFFolderBrowser;
using Newtonsoft.Json;
using Microsoft.Win32;

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

            LoadFileTypes();
            
            ComboBoxItem item;
            //File type (extension)
            foreach (string type in fileTypes) {
                item = new ComboBoxItem
                {
                    Content = type
                };
                this.ComboBox1.Items.Add(item);
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Destinations)) {
                FillWithSavedData();
            }
        }

        private void LoadFileTypes()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SystemFileTypes))
            {
                fileTypes = JsonConvert.DeserializeObject<string[]>(Properties.Settings.Default.SystemFileTypes);
            }
            else
            {
                this.fileTypes = GetSystemFileTypes();
                Properties.Settings.Default.SystemFileTypes = JsonConvert.SerializeObject(this.fileTypes);
                Properties.Settings.Default.Save();
            }
        }


        private void FillWithSavedData()
        {
            //extension = key, destination folder = value
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Settings.Default.Destinations);
            int i = 1;
            foreach (string key in values.Keys)
            {
                this.textBoxes["TextBox" + i].Text = values[key];
                this.comboBoxes["ComboBox" + i].Text = key;
                AddRow();
                ++i;
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
            AddRow();
        }

        private void AddRow() {
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
            comboBox.DropDownClosed += ComboBox_DropDownClosed;
            SelectPanel.Children.Add(comboBox);
            comboBoxes["ComboBox" + num] = comboBox;
        }


        //extension, destination folder
        private Dictionary<string, string> GetArgs() {
            var args = new Dictionary<string, string>();
            for(int i = 1; i <= textBoxes.Count; ++i)
            {
                //If destinatoin folder text and selected extension are not null
                if (!string.IsNullOrEmpty(textBoxes["TextBox" + i].Text) && !string.IsNullOrEmpty(comboBoxes["ComboBox" + i].Text))
                    args[comboBoxes["ComboBox" + i].Text] = textBoxes["TextBox" + i].Text;
            }
            return args;
        }
        

        //Remove destination directory
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
            this.deleteMode = true;
        }

        //Start sorting
        private async void Button_Click_2(object sender, RoutedEventArgs e)
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
            if (locations.Count != 0)
            {
                var progress = new Progress<int>(value => prgBar.Value = value);
                Mover mover;
                if ((bool)this.subdirectoriesBox.IsChecked)
                    mover = new Mover(locations, System.IO.SearchOption.AllDirectories);
                else
                    mover = new Mover(locations, System.IO.SearchOption.TopDirectoryOnly);
                await Task.Run(() => mover.Move(progress));
                MessageBox.Show("File moved successfully", "Operation completed", MessageBoxButton.OK, MessageBoxImage.Information);


            }
        }

        //Start sorting service
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("FileSorterService");
            //args go here, I have to make these arguments
            serviceController.Start();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //My key
            Properties.Settings.Default.Destinations = JsonConvert.SerializeObject(GetArgs(), Formatting.Indented);
            Properties.Settings.Default.Save();
        }

        //Calls upon combobox selection change, so that what user selected wasnt selected before
        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if ((sender as ComboBox).Name.Contains('1'))
            {
                return;
            }
            string newVal = (sender as ComboBox).Text;
            for (int i = 1; i <= (comboBoxes.Count - 1); ++i)
            {
                if (comboBoxes["ComboBox" + i].Text == newVal)
                {
                    MessageBox.Show("This has already been selected", "Invalid option", MessageBoxButton.OK, MessageBoxImage.Warning);
                    (sender as ComboBox).Text = null;
                }
            }
        }
    }
}
