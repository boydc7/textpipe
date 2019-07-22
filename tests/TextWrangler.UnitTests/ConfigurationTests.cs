using System;
using Newtonsoft.Json;
using NUnit.Framework;
using TextWrangler.Configuration;

namespace TextWrangler.UnitTests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void CanParseDefaultConfigCorrectly()
        {
            var testConfig = RecordConfigurationBuilder.Build("recordSample");

            var expectedJson = JsonConvert.SerializeObject(UnitTestSetup.SampleTestRecordConfiguration, Formatting.None);
            var actualJson = JsonConvert.SerializeObject(testConfig, Formatting.None);

            Assert.That(actualJson.Equals(expectedJson, StringComparison.Ordinal));
        }
    }
}
