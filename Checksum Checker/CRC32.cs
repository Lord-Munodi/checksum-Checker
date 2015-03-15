using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#if ALLOW_CRC_HASH
// This is just an example of how to add a hash function not supplied by Microsoft.
// CRC32 is not a particularly good hash so it's disabled with the preprocessor by default
namespace Checksum_Checker
{
    sealed class CRC32 : HashAlgorithm
    {
        private static uint[] lookupTable = createTable();
        private static uint[] createTable()
        {
            uint[] table = new uint[256];
            
            for(uint index = 0; index < 256; ++index)
            {
                uint c = index;
                for(uint k = 0; k < 8; ++k)
                {
                    if((c & 1) == 1)
                        c = (c >> 1) ^ 0xedb88320U;
                    else
                        c = c >> 1;
                }
                table[index] = c;
            }
            return table;
        }
        private UInt32 hashsum;

        public CRC32()
        {
            HashSizeValue = 32;
            Initialize();
        }

        public static new CRC32 Create()
        {
            return new CRC32();
        }

        public override void Initialize()
        {
            hashsum = 0xffffffff;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for(int cbUsed = 0; cbUsed < cbSize; ++cbUsed)
            {
                hashsum = lookupTable[(hashsum ^ array[ibStart + cbUsed]) & 0xff] ^ (hashsum >> 8);
            }
        }

        protected override byte[] HashFinal()
        {
            byte[] bytes = BitConverter.GetBytes(~hashsum);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
#endif // ALLOW_CRC_HASH