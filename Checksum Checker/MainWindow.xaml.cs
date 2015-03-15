using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private int calculating = 0;

        private List<HashItem> hashesSupported;
        public MainWindow(string fileName, List<string> commands)
        {
            InitializeComponent();

            hashesSupported = new List<HashItem>();
            hashesSupported.Add(new HashItem("md5", "MD5", typeof(MD5)));
            hashesSupported.Add(new HashItem("sha1", "SHA-1", typeof(SHA1)));
            hashesSupported.Add(new HashItem("sha256", "SHA-256", typeof(SHA256)));
            hashesSupported.Add(new HashItem("sha512", "SHA-512", typeof(SHA512)));
#if ALLOW_CRC_HASH
            hashesSupported.Add(new HashItem("crc32", "CRC32", typeof(CRC32)));
#endif
            Hashes.ItemsSource = hashesSupported;

            // Set file name and start all algorithms specified in commands
            FileNameInput.Text = fileName;
            foreach (string command in commands)
            {
                // Note this is experimental and may change, if you find it useful let me know so it's not removed without warning
                /*if (command.ToLower().Equals("--sha1"))
                    hashButton_Click(SHA1sumButton, new RoutedEventArgs());
                if (command.ToLower().Equals("--sha256"))
                    hashsumButton_Click(SHA256sumButton, new RoutedEventArgs());
                if (command.ToLower().Equals("--md5"))
                    hashsumButton_Click(MD5sumButton, new RoutedEventArgs());*/
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

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ((MenuItem)((ContextMenu)sender).Items[0]).IsEnabled = (calculating > 0);
            //((MenuItem)((ContextMenu)sender).Items[1]).Visibility = 
        }

        private void CancelAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var el in hashesSupported)
            {
                el.cancel();
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var el in hashesSupported)
            {
                el.Output = "";
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            string version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            MessageBox.Show("Checksum Checker version: " + version + "\n");
        }

        private int getSourceType()
        {
            return SourceSelector.SelectedIndex;
        }

        private async void HashButton_Click(object sender, RoutedEventArgs e)
        {
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
                    if (ex is IOException || ex is ArgumentException)
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
                if (StringInput.Text.Length == 0)
                {
                    MessageBox.Show("Invalid string, is empty.");
                    return;
                }
                if (StringInput.Text.Length % 2 == 1)
                {
                    MessageBox.Show("Invalid string, needs an even number of nibbles.");
                    return;
                }

                if (!Regex.IsMatch(StringInput.Text, @"^[a-fA-F0-9]+$"))
                {
                    MessageBox.Show("Invalid string, contains non-hex characters.");
                    return;
                }

                byte[] stringAsBytes = new byte[StringInput.Text.Length / 2];
                for (int i = 0; i < StringInput.Text.Length / 2; ++i)
                {
                    stringAsBytes[i] = Byte.Parse(StringInput.Text.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                }

                inStream = new MemoryStream(stringAsBytes, false);
            }

            HashItem item = hashesSupported.Find((x) => x.ShortName == ((string)((Button)sender).Tag));
            var node = ((Grid)((Button)sender).Parent);
            ProgressBar indicator = (ProgressBar)node.FindName("CalculatingIndicator");

            // Clear output and start calculating indicator
            item.Output = "";
            indicator.IsIndeterminate = true;
            indicator.Visibility = System.Windows.Visibility.Visible;
            ((Button)sender).IsEnabled = false;
            ++calculating;

            try
            {
                CancellationToken ctoken = item.CancellationToken;
                Func<Stream, HashAlgorithm, CancellationToken, string> func = (s, ha, ct) => App.hashStream(s, ha, ct);
                item.Output = await Task.Run(() => func(inStream, item.HashAlgorithm, ctoken));
            }
            catch (OperationCanceledException)
            {
                // This only happens when cancel is clicked. Do nothing special, just ignore it and execute after-exception code.
            }

            // Turn off computing indicators and fix height to make result visible
            --calculating;
            indicator.IsIndeterminate = false;
            indicator.Visibility = System.Windows.Visibility.Collapsed;
            ((Button)sender).IsEnabled = true;
            SizeToContent = System.Windows.SizeToContent.Height;
        }
    }
}
