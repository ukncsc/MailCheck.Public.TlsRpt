using System.IO;
using System.IO.Compression;

namespace MailCheck.TlsRpt.Reports.Processor.Compression
{
    public interface IDecompressor
    {
        string StreamType { get; }
        byte[] Decompress(Stream stream);
    }
    public interface IGZipDecompressor : IDecompressor { }

    public class GZipDecompressor : IGZipDecompressor
    {
        public string StreamType => "GZip";

        public byte[] Decompress(Stream stream)
        {
            using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (MemoryStream decompressedMemoryStream = new MemoryStream())
            {
                gzipStream.CopyTo(decompressedMemoryStream);
                return decompressedMemoryStream.ToArray();
            }
        }
    }
}