using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListViewTest
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        //private bool stopRefreshControls = false;
        private bool dataChanged = false;
        private HashSet<String> itemsSet = new HashSet<String>();
        public Window1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// addButton click event handler.
        /// Add a new row to the ListView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            //Browse media files
            OpenFileDialog dlg = new OpenFileDialog();
            //Allow multiple selections
            dlg.Multiselect = true;
            dlg.Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";

            if (dlg.ShowDialog() == true)
            {
                foreach(string fileName in dlg.FileNames)
                {
                    try
                    {
                        AddRow(fileName, "");
                        setDataChanged(true);
                    }
                    catch (SecurityException ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the file: " + fileName.Substring(fileName.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
                listView1.Focus();
            }
        }

        /// <summary>
        /// Adds a row to the ListView and it overwrites the row if it already exists (same fileName)
        /// </summary>
        private void AddRow(string fileName, string lastGPSLocation)
        {
            //Remove the file name if it already exists in the list
            if(itemsSet.Contains(fileName))
            {
                //remove fileName from the hashset
                itemsSet.Remove(fileName);

                foreach(var item in listView1.Items)
                {
                    //remove item from listView
                    if(((ListViewData)item).Col1.Equals(fileName))
                    {
                        listView1.Items.Remove(item);
                        listView1.Items.Refresh();
                        break; //break after first remove, otherwise we run into an exception
                    }
                }
            }
            //Add the row or overwrite
            listView1.Items.Add(new ListViewData(fileName, lastGPSLocation));
            itemsSet.Add(fileName);

            setDataChanged(true);
            listView1.Items.Refresh();
        }


        /// <summary>
        /// removeButton click event handler
        /// Removes the selected row from the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            setDataChanged(true);

            //remove multiple selections
            var list = listView1.SelectedItems.Cast<Object>().ToArray();

            foreach (var item in list)
            {
                itemsSet.Remove(((ListViewData)item)?.Col1);

                listView1.Items.Remove(item);
            }

            listView1.Items.Refresh();
        }



        /// <summary>
        /// textBox2 TextChanged event handler
        /// Updates the ListView row with current Text 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            //RefreshListView(textBox1.Text, textBox2.Text);
        }

        /// <summary>
        /// listView1 SelectionChnaged event handler.
        /// Updates the textboxes with values in the row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Window Loaded event handler
        /// Loads data into ListView, and selecta a row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowData();

            setDataChanged(false);
            listView1.Focus();
        }

        /// <summary>
        /// Shows(Loads) data into the ListView
        /// </summary>
        private void ShowData()
        {
            MyData md = new MyData();
            listView1.Items.Clear();

            foreach (var row in md.GetRows())
            {
                listView1.Items.Add(row);
                //maintain items in the hashset to flag duplicates
                itemsSet.Add(((ListViewData)row).Col1);
            }
        }

        /// <summary>
        /// saveButton click event handler.
        /// Saves data from ListView, if it is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveButton.IsEnabled = false;
            
            if (dataChanged)
            {
                MyData md = new MyData();
                md.Save(listView1.Items);
                setDataChanged(false);
            }
        }


        /// <summary>
        /// okButton click event handler.
        /// Saves the data, and closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            okButton.IsEnabled = false;
            if (dataChanged)
            {
                MyData md = new MyData();
                md.Save(listView1.Items);
            }
            this.Close();
        }

        /// <summary>
        /// Sets the window into a DataChanged status.
        /// </summary>
        /// <param name="value"></param>
        private void setDataChanged(bool value)
        {
            dataChanged = value;
            saveButton.IsEnabled = value;
        }

        /// <summary>
        /// Window closing event handler.
        /// Prompts the user to save data, if it is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dataChanged)
            {
                string message = "Your changes are not saved. Do you want to save it now?";
                MessageBoxResult result = MessageBox.Show(message,
                        this.Title,
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    MyData md = new MyData();
                    md.Save(listView1.Items);
                    setDataChanged(false);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    // do nothing
                }
            }
        }
    }
}
