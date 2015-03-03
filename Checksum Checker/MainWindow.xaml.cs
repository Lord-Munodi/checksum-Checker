using System;
using System.Collections.Generic;
using System.Text;
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
            Func<string, CancellationToken, string> func;
            if(sender == SHA1sumButton)
            {
                cTokenSource = sha1TokenSource;
                progressBar = SHA1Progress;
                outputTextBox = SHA1Output;
                func = (x, y) => App.SHA1File(x, y);
            }
            else if (sender == SHA256sumButton)
            {
                cTokenSource = sha256TokenSource;
                progressBar = SHA256Progress;
                outputTextBox = SHA256Output;
                func = (x, y) => App.SHA256File(x, y);
            }
            else if (sender == MD5sumButton)
            {
                cTokenSource = md5TokenSource;
                progressBar = MD5Progress;
                outputTextBox = MD5Output;
                func = (x, y) => App.MD5File(x, y);
            }
            else
            {
                throw new ArgumentException("Button " + ((Button)sender).Content + " not implemented");
            }

            // Here the references set earlier are used
            if (FileNameInput.Text != null && FileNameInput.Text.Length > 0)
            {
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
                    outputTextBox.Text = await Task.Run(() => func(text, token), token);
                }
                catch (OperationCanceledException)
                {
                    // Catch exception and return so as not to stop progress indicator,this is because this
                    // exception is thrown when the hash button is reclicked which means the next calculation is running
                    return;
                }

                // Turn off progress indicator and fix height to make result visible
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                SizeToContent = System.Windows.SizeToContent.Height;
            }
        }

        private void SourceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SourceSelector.SelectedIndex == 1)  // String selected, collapse file, expand string
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Visible;
                FileInputInputter.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (SourceSelector.SelectedIndex == 0) // File selected, collapse string, expand file
            {
                StringInputInputter.Visibility = System.Windows.Visibility.Collapsed;
                FileInputInputter.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
