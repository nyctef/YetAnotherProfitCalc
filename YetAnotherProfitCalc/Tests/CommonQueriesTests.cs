using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc.Tests
{
    class CommonQueriesTests
    {
        [TestCase(0, 10, Result = 0.1)]
        [TestCase(-1, 10, Result = 0.2)]
        [TestCase(-4, 10, Result = 0.5)]
        [TestCase(1, 10, Result = 0.05)]
        [TestCase(2, 10, Result = 0.1 / 3)]
        [TestCase(0, 5, Result = 0.05)]
        public decimal TestGetWasteForME(int ME, int wasteFactor = 10)
        {
            return CommonQueries.GetWasteForME(ME, wasteFactor);
        }

        public void TestGetAttributesForType()
        {
            var attributes = CommonQueries.GetAttributesForType(new TypeID(23757));
            foreach (var attr in attributes)
            {
                Console.WriteLine(attr.Attribute.DisplayName + ": " + attr.Value);
            }
            Assert.AreEqual(9, attributes.Count, "Count should be 9");
        }
    }

    class FetchEveCentralPriceTests
    {
        [TestCase(25601)]
        public void Test(int typeId)
        {
            Console.WriteLine(FetchEveCentralPrice.FetchPrice(typeId));
        }
    }
}
