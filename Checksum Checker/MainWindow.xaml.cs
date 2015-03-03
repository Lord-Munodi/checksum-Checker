using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace Checksum_Checker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                FileNameInput.Text = args[1];

            //SHA1Output.Text = new String(' ', int.Parse(SHA1Output.Tag.ToString()));
            //SHA256Output.Text = new String(' ', int.Parse(SHA256Output.Tag.ToString()));
            //SHA1Output.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            var dlg = new Microsoft.Win32.OpenFileDialog();

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                FileNameInput.Text = filename;
            }
        }

        private CancellationTokenSource sha1TokenSource = new CancellationTokenSource();
        private CancellationTokenSource sha256TokenSource = new CancellationTokenSource();
        private CancellationTokenSource md5TokenSource = new CancellationTokenSource();

        private async void hashsumButton_Click(object sender, RoutedEventArgs e)
        {
            // Figure out which hashing button was clicked and set references to controls and functions accordingly
            CancellationTokenSource cTokenSource;
            ProgressBar progressBar;
            TextBox outputTextBox;
            Func<Stream, CancellationToken, string> func;
            if(sender == SHA1sumButton)
            {
                cTokenSource = sha1TokenSource;
                progressBar = SHA1Progress;
                outputTextBox = SHA1Output;
                func = (x, y) => App.hashStream(x, SHA1.Create(), y);
            }
            else if (sender == SHA256sumButton)
            {
                cTokenSource = sha256TokenSource;
                progressBar = SHA256Progress;
                outputTextBox = SHA256Output;
                func = (x, y) => App.hashStream(x, SHA256.Create(), y);
            }
            else if (sender == MD5sumButton)
            {
                cTokenSource = md5TokenSource;
                progressBar = MD5Progress;
                outputTextBox = MD5Output;
                func = (x, y) => App.hashStream(x, MD5.Create(), y);
            }
            else
            {
                throw new ArgumentException("Button " + ((Button)sender).Content + " not implemented");
            }

            Stream inStream;

            if (getSourceType() == "File")
            {
                try
                {
                    inStream = new FileStream(FileNameInput.Text, FileMode.Open, FileAccess.Read);
                }
                catch (Exception ex)
                {
                    if(ex is IOException || ex is ArgumentException)
                    {
                        Keyboard.Focus(FileNameInput);
                        FileNameInput.SelectAll();
                        MessageBox.Show("Cannot open file");
                    }
                    else
                    {
                        App.ReportExceptionMessage(ex);
                    }
                    return;
                }
            }
            else
            {
                if(StringInput.Text.Length == 0)
                {
                    MessageBox.Show("Invalid string, is empty.");
                    return;
                }
                if(StringInput.Text.Length % 2 == 1)
                {
                    MessageBox.Show("Invalid string, needs an even number of nibbles.");
                    return;
                }

                if(!Regex.IsMatch(StringInput.Text, @"^[a-fA-F0-9]+$"))
                {
                    MessageBox.Show("Invalid string, contains non-hex characters.");
                    return;
                }

                byte[] stringAsBytes = new byte[StringInput.Text.Length / 2];
                for(int i = 0; i < StringInput.Text.Length / 2; ++i)
                {
                    stringAsBytes[i] = Byte.Parse(StringInput.Text.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                }

                //Console.WriteLine(StringInput.Text);
                //foreach (byte b in stringAsBytes)
                //    Console.Write(b);
                //Console.WriteLine();
                
                inStream = new MemoryStream(stringAsBytes, false);
            }

            // Here the references set earlier are used
            
                // Cancel a previous hash if it's still running, this means repeatedly clicking restarts the hashing
                cTokenSource.Cancel();
                cTokenSource = new CancellationTokenSource();
                CancellationToken token = cTokenSource.Token;

                // Clear output and start calculating indicator
                outputTextBox.Text = "";// new String(' ', int.Parse(outputTextBox.Tag.ToString()));
                progressBar.IsIndeterminate = true;
                progressBar.Visibility = System.Windows.Visibility.Visible;

                String text = FileNameInput.Text;

                try
                {
                    outputTextBox.Text = await Task.Run(() => func(inStream, token), token);
                }
                catch (OperationCanceledException)
                {
                    // Catch exception and return so as not to stop progress indicator,this is because this
                    // exception is thrown when the hash button is reclicked which means the next calculation is running
                    return;
                }
                catch (FileNotFoundException)
                {
                    Keyboard.Focus(FileNameInput);
                    FileNameInput.SelectAll();
                }
                

                // Turn off progress indicator and fix height to make result visible
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                SizeToContent = System.Windows.SizeToContent.Height;
        }

        private void SourceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (getSourceType() == "String")  // String selected, collapse file, expand string
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Visible;
                FileInputInputter.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (getSourceType() == "File") // File selected, collapse string, expand file
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Collapsed;
                FileInputInputter.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private String getSourceType()
        {
            return SourceSelector.SelectedIndex == 0 ? "File" : "String";
        }
    }
}
