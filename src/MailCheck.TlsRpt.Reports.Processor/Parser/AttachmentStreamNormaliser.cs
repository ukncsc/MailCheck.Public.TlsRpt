using System;
using System.IO;
using System.Linq;
using MailCheck.TlsRpt.Reports.Processor.Compression;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace MailCheck.TlsRpt.Reports.Processor.Parser
{
    public interface IAttachmentStreamNormaliser
    {
        AttachmentInfo Normalise(MimePart mimePart);
    }

    public class AttachmentStreamNormaliser : IAttachmentStreamNormaliser
    {
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly IGZipDecompressor _gZipDecompressor;
        private readonly IZipDecompressor _zipDecompressor;
        private readonly ILogger<AttachmentStreamNormaliser> _log;

        public AttachmentStreamNormaliser(IContentTypeProvider contentTypeProvider, IGZipDecompressor gZipDecompressor, IZipDecompressor zipDecompressor, ILogger<AttachmentStreamNormaliser> log)
        {
            _contentTypeProvider = contentTypeProvider;
            _gZipDecompressor = gZipDecompressor;
            _zipDecompressor = zipDecompressor;
            _log = log;
        }

        public AttachmentInfo Normalise(MimePart mimePart)
        {
            byte[] normalisedStream = NormaliseStream(mimePart);

            return normalisedStream == null
                ? AttachmentInfo.EmptyAttachmentInfo
                : new AttachmentInfo(mimePart.FileName, normalisedStream);
        }

        private byte[] NormaliseStream(MimePart mimePart)
        {
            string contentType = _contentTypeProvider.GetContentType(mimePart);

            switch (contentType)
            {
                case ContentType.ApplicationGzip:
                case ContentType.TlsRptGzip:
                    return Decompress(mimePart, _gZipDecompressor, _zipDecompressor);
                case ContentType.ApplicationXZipCompressed:
                case ContentType.ApplicationZip:
                    return Decompress(mimePart, _zipDecompressor, _gZipDecompressor);
                case ContentType.ApplicationOctetStream:
                    string extension = Path.GetExtension(mimePart.FileName.Split('!').LastOrDefault());
                    if (extension?.StartsWith(".z") ?? false)
                    {
                        return Decompress(mimePart, _zipDecompressor, _gZipDecompressor);
                    }
                    return Decompress(mimePart, _gZipDecompressor, _zipDecompressor);
                case ContentType.TextXml:
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        mimePart.Content.DecodeTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return memoryStream.ToArray();
                    }
                default:
                    return null;
            }
        }

        private byte[] Decompress(MimePart mimePart, params IDecompressor[] decompressors)
        {
            foreach (IDecompressor decompressor in decompressors)
            {
                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        mimePart.Content.DecodeTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        byte[] decompressedStream = decompressor.Decompress(memoryStream);

                        _log.LogInformation($"Successfully decompressed {mimePart.FileName ?? "<null>"} as {decompressor.StreamType}.");

                        return decompressedStream;
                    }
                }
                catch (Exception)
                {
                    _log.LogInformation($"Failed to decompress {mimePart.FileName ?? "<null>"} as {decompressor.StreamType}.");
                }
            }
            return null;
        }
    }
}