using System;
using System.IO;
using FakeItEasy;
using MailCheck.TlsRpt.Reports.Processor.Compression;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using Microsoft.Extensions.Logging;
using MimeKit;
using NUnit.Framework;
using ContentType = MimeKit.ContentType;

namespace MailCheck.TlsRpt.Reports.Processor.Test.Parser
{
    [TestFixture]
    public class AttachmentStreamNormaliserTests
    {
        private const string ApplicationMediaType = "application";

        private const string GzipMediaSubType = "gzip";
        private const string ZipMediaSubType = "zip";
        private const string OctetStreamMediaSub = "octet-stream";

        private AttachmentStreamNormaliser _attachmentStreamNormaliser;
        private IContentTypeProvider _contentTypeProvider;
        private IGZipDecompressor _gZipDecompressor;
        private IZipDecompressor _zipDecompressor;

        [SetUp]
        public void SetUp()
        {
            _contentTypeProvider = A.Fake<IContentTypeProvider>();
            _gZipDecompressor = A.Fake<IGZipDecompressor>();
            _zipDecompressor = A.Fake<IZipDecompressor>();
            _attachmentStreamNormaliser = new AttachmentStreamNormaliser(_contentTypeProvider, _gZipDecompressor, _zipDecompressor, A.Fake<ILogger<AttachmentStreamNormaliser>>());
        }

        [TestCase(ApplicationMediaType, GzipMediaSubType, "test.gz", true, false, TestName = "application/gzip decompressed as gzip")]
        [TestCase(ApplicationMediaType, ZipMediaSubType, "test.zip", false, true, TestName = "application/zip decompressed as zip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.gz", true, false, TestName = "application/octet-stream with gz extension decompressed as gzip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.zip", false, true, TestName = "application/octet-stream with zip extension decompressed as zip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.z", false, true, true, true, true, TestName = "application/octet-stream with extension starting with z decompressed as zip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.g", true, false, true, true, true, TestName = "application/octet-stream with extension starting with g decompressed as gzip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.a", true, false, true, true, true, TestName = "application/octet-stream with extension not starting with z decompressed as gzip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test", true, false, true, true, true, TestName = "application/octet-stream with no extension decompressed as gzip")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.gz", true, true, false, true, true, TestName = "application/octet-stream with gz extension decompressed as zip when gzip fails")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.zip", true, true, true, false, true, TestName = "application/octet-stream with zip extension decompressed as gzip when zip fails")]
        [TestCase(ApplicationMediaType, OctetStreamMediaSub, "test.zip", true, true, false, false, false, TestName = "zip and gzip fail then empty attachment info returned")]
        public void Test(string mediaType, string mediaSubType, string fileName, bool gZipCalled, bool zipCalled, bool gzipSuccess = true, bool zipSuccess = true, bool success = true)
        {
            MimePart mimePart = CreateMimePart(mediaType, mediaSubType, fileName);
            A.CallTo(() => _contentTypeProvider.GetContentType(A<MimePart>._)).Returns($@"{mediaType}/{mediaSubType}");

            if (!gzipSuccess)
            {
                A.CallTo(() => _gZipDecompressor.Decompress(A<Stream>._)).Throws<Exception>();
            }

            if (!zipSuccess)
            {
                A.CallTo(() => _zipDecompressor.Decompress(A<Stream>._)).Throws<Exception>();
            }

            AttachmentInfo attachment = _attachmentStreamNormaliser.Normalise(mimePart);

            if (gZipCalled)
            {
                A.CallTo(() => _gZipDecompressor.Decompress(A<Stream>._)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _gZipDecompressor.Decompress(A<Stream>._)).MustNotHaveHappened();
            }

            if (zipCalled)
            {
                A.CallTo(() => _zipDecompressor.Decompress(A<Stream>._)).MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _zipDecompressor.Decompress(A<Stream>._)).MustNotHaveHappened();
            }

            if (success)
            {
                Assert.That(attachment, Is.Not.Null);
                Assert.That(attachment.Filename, Is.EqualTo(mimePart.FileName));
            }
            else
            {
                Assert.That(attachment, Is.EqualTo(AttachmentInfo.EmptyAttachmentInfo));
            }
        }

        private MimePart CreateMimePart(string mediaType, string mediaSubtype, string filename, params Header[] headers)
        {
            ContentType contentType = new MimeKit.ContentType(mediaType, mediaSubtype);

            IMimeContent contentObject = A.Fake<IMimeContent>();
            A.CallTo(() => contentObject.Stream).Returns(new MemoryStream());

            MimePart mimePart = new MimePart(contentType)
            {
                FileName = filename,
                Content = contentObject
            };

            foreach (Header header in headers)
            {
                mimePart.Headers.Add(header);
            }

            return mimePart;
        }
    }
}
