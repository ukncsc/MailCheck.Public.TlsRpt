﻿using System.IO;
using System.Linq;
using MailCheck.TlsRpt.Reports.Processor.Compression;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test.Compression
{
    [TestFixture]
    public class GzipDecompressorTests
    {
        private static readonly byte[] GZippedData = { 0x1f, 0x8b, 0x08, 0x08, 0x07, 0xde, 0x47, 0x58, 0x00, 0x03, 0x74, 0x65, 0x73, 0x74, 0x64, 0x61, 0x74, 0x61, 0x00, 0x73, 0xaf, 0xca, 0x2c, 0x70, 0x49, 0x4d, 0xce, 0xcf, 0x2d, 0x28, 0x4a, 0x2d, 0x2e, 0xce, 0xcc, 0xcf, 0x0b, 0x49, 0x2d, 0x2e, 0xe1, 0x02, 0x00, 0x9c, 0xa2, 0xed, 0x71, 0x16, 0x00, 0x00, 0x00 };
        private static readonly byte[] NonGZippedData = { 0x47, 0x5a, 0x69, 0x70, 0x44, 0x65, 0x63, 0x6f, 0x6d, 0x70, 0x72, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x54, 0x65, 0x73, 0x74, 0x0a };

        private GZipDecompressor _gzipDecompressor;

        [SetUp]
        public void SetUp()
        {
            _gzipDecompressor = new GZipDecompressor();
        }

        [Test]
        public void GzipCompressedStreamIsSuccessfullyDecompressed()
        {
            Stream compressedStream = new MemoryStream(GZippedData);
            byte[] decompressedStream = _gzipDecompressor.Decompress(compressedStream).ToArray();

            Assert.That(decompressedStream, Is.EqualTo("GzipDecompressionTest\n"));
        }

        [Test]
        public void NonGzippedStreamThrowsInvalidDataException()
        {
            Stream nonCompressedStream = new MemoryStream(NonGZippedData);

            Assert.Throws<InvalidDataException>(() => _gzipDecompressor.Decompress(nonCompressedStream));
        }
    }
}