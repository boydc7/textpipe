using NUnit.Framework;
using TextWrangler.Models;
using TextWrangler.Services.Filters;

namespace TextWrangler.UnitTests
{
    [TestFixture]
    public class FieldFilterTests
    {
        [Test]
        public void TrimFilterTrimsSimpleStringFromBothSides()
        {
            const string trimedString = @"first test
but not the end yet




end of it all";

            const string testString = @"





" + trimedString + @"



            ";

            var filterredString = TrimFilter.Instance.Filter(testString);

            Assert.AreEqual(trimedString, filterredString);
        }

        private void AssertNullStringIsHanldedByFilter(IFieldFilter filter)
        {
            string nullString = null;

            var filterredString = filter.Filter(nullString);

            Assert.IsNull(filterredString);
        }

        [Test]
        public void TrimFilterIgnoresNullStringWithoutFailure()
        {
            AssertNullStringIsHanldedByFilter(TrimFilter.Instance);
        }

        [Test]
        public void UpperFilterUpperCasesMixedStringCorrectly()
        {
            const string testString = "first test 213-8093~89)#*$&)(# some more";
            const string expected = "FIRST TEST 213-8093~89)#*$&)(# SOME MORE";

            var filteredString = UpperFilter.Instance.Filter(testString);

            Assert.AreEqual(expected, filteredString);
        }

        [Test]
        public void UpperFilterIgnoresNullStringWithoutFailure()
        {
            AssertNullStringIsHanldedByFilter(UpperFilter.Instance);
        }

        [Test]
        public void AlphaFilterDoesNotFailOnAlphaOnlyString()
        {
            const string testString = "TheQuickBrownFoxJumpedOverTheLazyDog";

            var filteredString = AlphaFilter.Instance.Filter(testString);

            Assert.AreEqual(testString, filteredString);
        }

        [Test]
        public void AlphaFilterThrowsExceptionOnNonAlphaOnlyString()
        {
            const string testString = "The Quick Brown Fox Jumped Over The Lazy Dog";

            Assert.Throws<TextWranglerFieldFilterException>(() => AlphaFilter.Instance.Filter(testString));
        }

        [Test]
        public void AlphaFilterIgnoresNullStringWithoutFailure()
        {
            AssertNullStringIsHanldedByFilter(AlphaFilter.Instance);
        }

        [Test]
        public void AlphaNumericFilterFilterDoesNotFailOnAlphaNumericOnlyString()
        {
            const string testString = "The123Quick456BrownFoxJumpedOverTheLazyDog789";

            var filteredString = AlphaNumericFilter.Instance.Filter(testString);

            Assert.AreEqual(testString, filteredString);
        }

        [Test]
        public void AlphaNumericFilterThrowsExceptionOnNonAlphaNumericOnlyString()
        {
            const string testString = "The Quick Brown Fox Jumped Over The Lazy Dog";

            Assert.Throws<TextWranglerFieldFilterException>(() => AlphaNumericFilter.Instance.Filter(testString));
        }

        [Test]
        public void AlphaNumericFilterIgnoresNullStringWithoutFailure()
        {
            AssertNullStringIsHanldedByFilter(AlphaNumericFilter.Instance);
        }
    }
}
