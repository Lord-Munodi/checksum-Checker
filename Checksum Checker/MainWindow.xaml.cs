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
        private const int SOURCE_IS_FILE = 0;
        private const int SOURCE_IS_STRING = 1;
        public MainWindow(string fileName, List<string> commands)
        {
            InitializeComponent();

            // Set file name and start all algorithms specified in commands
            FileNameInput.Text = fileName;
            foreach (string command in commands)
            {
                // Note this is experimental and may change, if you find it useful let me know so it's not removed without warning
                if (command.ToLower().Equals("-sha1"))
                    hashsumButton_Click(SHA1sumButton, new RoutedEventArgs());
                if (command.ToLower().Equals("-sha256"))
                    hashsumButton_Click(SHA256sumButton, new RoutedEventArgs());
                if (command.ToLower().Equals("-md5"))
                    hashsumButton_Click(MD5sumButton, new RoutedEventArgs());
            }
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog box and set FileNameInput's text to the full name of selected file
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All Files|*.*|Disc image|*.img; *.iso|Programs|*.exe; *.msi|Compressed files|*.bz2; *.gz; *.zip;|Video|*.flv; *.mp4; *.mkv"; // Filter files by extension 
            Nullable<bool> result = dlg.ShowDialog(this);
            if (result == true)
                FileNameInput.Text = dlg.FileName;
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
            // Set inStream to either a FileStream or MemoryStream depending on input source
            if (getSourceType() == SOURCE_IS_FILE)
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
                
                inStream = new MemoryStream(stringAsBytes, false);
            }

            // Here the references set earlier are used
            
            CancellationToken token = cTokenSource.Token;

            // Clear output and start calculating indicator
            outputTextBox.Text = "";
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = System.Windows.Visibility.Visible;
            ((Button)sender).IsEnabled = false;

            String text = FileNameInput.Text;

            try
            {
                outputTextBox.Text = await Task.Run(() => func(inStream, token), token);
            }
            catch (OperationCanceledException)
            {
                // This only happens when cancel is clicked. Do nothing special, just ignore it and execute after-exception code.
            }   

            // Turn off computing indicators and fix height to make result visible
            progressBar.IsIndeterminate = false;
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
            ((Button)sender).IsEnabled = true;
            SizeToContent = System.Windows.SizeToContent.Height;
        }

        private void SourceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (getSourceType() == SOURCE_IS_STRING)  // String selected, collapse file, expand string
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Visible;
                FileInputInputter.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (getSourceType() == SOURCE_IS_FILE) // File selected, collapse string, expand file
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Collapsed;
                FileInputInputter.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            sha1TokenSource.Cancel();
            sha256TokenSource.Cancel();
            md5TokenSource.Cancel();
            sha1TokenSource = new CancellationTokenSource();
            sha256TokenSource = new CancellationTokenSource();
            md5TokenSource = new CancellationTokenSource();
            SHA1Output.Clear();
            SHA256Output.Clear();
            MD5Output.Clear();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            string version = assembly.GetName().Version.ToString();
            while (version.EndsWith(".0"))
                version = version.Substring(0, version.Length - 2);

            MessageBox.Show("Checksum Checker v." + version + "\n");
        }

        private int getSourceType()
        {
            return SourceSelector.SelectedIndex;
        }
    }
}
