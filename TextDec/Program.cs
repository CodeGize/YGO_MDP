using System.IO;
using System.IO.Compression;
namespace TextDec
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "enc":
                        Enc(args);
                        break;
                    case "dec":
                        Dec(args);
                        break;
                }
            }
        }

        private static void Enc(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                var fname = args[i];
                using (FileStream fs = File.OpenRead(fname))
                using (DeflateStream ds = new DeflateStream(fs, CompressionMode.Compress))
                {
                    var bytes = new byte[ds.Length];
                    ds.Read(bytes, 0, (int)ds.Length);
                    Xor(bytes);
                    File.WriteAllBytes(fname + ".patch", bytes);
                }
            }
        }

        private static void Dec(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                var fname = args[i];
                byte[] data = File.ReadAllBytes(fname);
                Xor(data);

                using (MemoryStream ms = new MemoryStream(data))
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                using (FileStream fs = File.Create(fname + ".dec"))
                {
                    //ms.Position = 2;
                    ds.CopyTo(fs);
                }
            }
        }

        static void Xor(byte[] data)
        {
            const byte m_iCryptoKey = 0xda;//0xB3;

            for (int i = 0; i < data.Length; i++)
            {
                byte v = (byte)((i + m_iCryptoKey + 0x23D) * m_iCryptoKey);
                v ^= (byte)(i % 7);
                data[i] ^= v;
            }
        }
    }
}
