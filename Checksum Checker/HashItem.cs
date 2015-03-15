using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Checksum_Checker
{
    public class HashItem : INotifyPropertyChanged
    {
        private readonly string shortName;
        private CancellationTokenSource cTokenSource;
        private readonly string buttonLabel;
        private Type hashAlgorithm;
        private string output = "";

        // Declare the event 
        public event PropertyChangedEventHandler PropertyChanged;

        public string ShortName { get { return shortName; } }
        public string ButtonLabel { get { return buttonLabel; } }
        public HashAlgorithm HashAlgorithm
        {
            get
            {
                var createMethod = hashAlgorithm.GetMethod("Create", Type.EmptyTypes);
                return (HashAlgorithm)(createMethod.Invoke(null, null));
            }
        }
        public CancellationToken CancellationToken { get { return cTokenSource.Token; } }
        public string Output { get { return output; } set { output = value; OnPropertyChanged("Output"); } }

        public HashItem(string shortName, string label, Type hashAlgo)
        {
            this.shortName = shortName;
            buttonLabel = label;
            this.hashAlgorithm = hashAlgo;
            cTokenSource = new CancellationTokenSource();
        }

        public void cancel()
        {
            cTokenSource.Cancel();
            cTokenSource = new CancellationTokenSource();
        }

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /*public string calculateHash(Stream stream)
        {
            var createMethod = hashAlgorithm.GetMethod("Create", Type.EmptyTypes);
            HashAlgorithm hashAlgo = (HashAlgorithm)(createMethod.Invoke(null, null));
            var token = cTokenSource.Token;

            const int READ_CHUNK_SIZE = 4096;
            byte[] read = new byte[READ_CHUNK_SIZE];
            int bytesRead;
            do
            {
                token.ThrowIfCancellationRequested();
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
        }*/
    }
}
