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
        private const int READ_CHUNK_SIZE = 4096;

        public static string hashStream(Stream stream, HashAlgorithm hashAlgo, CancellationToken tok = default(CancellationToken))
        {
            BinaryReader infile = new BinaryReader(stream);

            byte[] read;
            do
            {
                tok.ThrowIfCancellationRequested();
                read = infile.ReadBytes(READ_CHUNK_SIZE);
                hashAlgo.TransformBlock(read, 0, read.Length, null, 0);
            } while (read.Length == READ_CHUNK_SIZE);

            infile.Close();
            hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
            return BytesToHexString(hashAlgo.Hash);
        }

        private static string BytesToHexString(byte[] bytes)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
                str.AppendFormat("{0:x2}", bytes[i]);

            return str.ToString();
        }

        public static void ReportExceptionMessage(Exception e)
        {
            MessageBox.Show("Something unexpected happened. Please report it. \n\n" + e.ToString());
        }
    }
}
