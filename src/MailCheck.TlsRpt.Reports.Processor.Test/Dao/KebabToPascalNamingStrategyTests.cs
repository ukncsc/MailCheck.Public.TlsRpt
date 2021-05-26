using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.TlsRpt.Reports.Processor.Dao;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test.Dao
{
    [TestFixture]
    public class KebabToPascalNamingStrategyTests
    {
        private KebabToPascalNamingStrategy _kebabToPascalNamingStrategy;

        [SetUp]
        public void SetUp()
        {
            _kebabToPascalNamingStrategy = new KebabToPascalNamingStrategy();
        }

        [TestCase("example-value", "ExampleValue")]
        [TestCase("EXAMPLE-VALUE", "ExampleValue")]
        [TestCase("Example-Value", "ExampleValue")]
        [TestCase("Example-value", "ExampleValue")]
        public void TransformHandlesKebabCasedValues(string input, string expectedOutput)
        {
            string result = _kebabToPascalNamingStrategy.GetDictionaryKey(input);
            Assert.AreEqual(expectedOutput, result);
        }
    }
}
