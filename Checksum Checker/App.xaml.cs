using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Checksum_Checker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // copy reference to command line arguments
            string[] args = Environment.GetCommandLineArgs();
            List<string> fileNames = new List<string>();
            List<string> commands = new List<string>();
            // for each argument, if begins with '-' add to commands, else add to fileNames
            for(int i = 1; i < args.Length; ++i)
            {
                (args[i].StartsWith("-") ? commands : fileNames).Add(args[i]);
            }
            // if no file name open window with no filename or commands
            if (fileNames.Count == 0)
            {
                fileNames.Add("");
                commands.Clear();
            }

            // for each file name open a window with the name and all the commands to each
            foreach (string fileName in fileNames)
            {
                MainWindow mainWindow = new MainWindow(fileName, commands);
                mainWindow.Show();
            }
        }

        public static string hashStream(Stream stream, HashAlgorithm hashAlgo, CancellationToken tok = default(CancellationToken))
        {
            const int READ_CHUNK_SIZE = 4096;
            byte[] read = new byte[READ_CHUNK_SIZE];
            int bytesRead;
            do
            {
                tok.ThrowIfCancellationRequested();
                bytesRead = stream.Read(read, 0, READ_CHUNK_SIZE);
                hashAlgo.TransformBlock(read, 0, bytesRead, null, 0);
            } while (bytesRead > 0);

            hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
            return bytesToHexString(hashAlgo.Hash);
        }

        private static string bytesToHexString(byte[] bytes)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
                str.AppendFormat("{0:x2}", b);

            return str.ToString();
        }

        public static void ReportExceptionMessage(Exception e)
        {
            MessageBox.Show("Something unexpected happened. Please report it. \n\n" + e.ToString());
        }
    }
}
