using System;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace TextWrangler.UnitTests
{
    [TestFixture]
    public class FrameworkTests
    {
        [Test]
        public void VariousSampleFormatStringsWorkAsExpected()
        {
            const double rawValue = 123_456_789.987654321;

            var fmt = string.Format("{0,-2:C}", rawValue);
            Assert.AreEqual("¤123,456,789.99", fmt, "0,-2:C");

            fmt = string.Format("{0,0:C2}", rawValue);
            Assert.AreEqual("¤123,456,789.99", fmt, "0,0:C2");

            fmt = string.Format("{0,0:N0}", rawValue);
            Assert.AreEqual("123,456,790", fmt, "0,0:N0");

            fmt = string.Format("{0,0:N5}", rawValue);
            Assert.AreEqual("123,456,789.98765", fmt, "0,0:N5");

            fmt = string.Format("{0,0:C}", rawValue);
            Assert.AreEqual("¤123,456,789.99", fmt, "0,0:C");

            fmt = string.Format("{0,0:C0}", rawValue);
            Assert.AreEqual("¤123,456,790", fmt, "0,0:C0");

            var dateToTest1 = new DateTime(2014, 11, 26);
            var dateToTest2 = new DateTime(2014, 8, 7);

            fmt = string.Format("{0,0:M/d/yy}", dateToTest1);
            Assert.AreEqual("11/26/14", fmt, "0,0:M/d/yy");

            fmt = string.Format("{0,0:M/d/yyyy}", dateToTest1);
            Assert.AreEqual("11/26/2014", fmt, "0,0:M/d/yyyy");

            fmt = string.Format("{0,0:M/d/yy}", dateToTest2);
            Assert.AreEqual("8/7/14", fmt, "0,0:M/d/yy");

            fmt = string.Format("{0,0:P2}", 0.87651598);
            Assert.AreEqual("87.65 %", fmt, "0,0:P2");
        }
    }
}
