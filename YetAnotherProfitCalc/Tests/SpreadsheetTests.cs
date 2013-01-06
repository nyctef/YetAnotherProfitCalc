using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc.Tests
{
    [TestFixture]
    public class SpreadsheetTests
    {
        [Test]
        public void SimpleTest()
        {
            var cell1 = new SimpleCell("1");
            var cell2 = new SimpleCell("2");
            var cell3 = new FormulaCell("={0}+{1}", cell1, cell2);

            var spreadsheet = new TSVSpreadsheet();
            spreadsheet.AddCell(cell1, 0, 0);
            spreadsheet.AddCell(cell2, 1, 0);
            spreadsheet.AddCell(cell3, 1, 1);

            Assert.AreEqual("1\t2\t\n\t=A1+B1\t\n", spreadsheet.Export());
        }

        [Test]
        public void ECCellTest()
        {
            var cell1 = new EveCentralCell(new TypeID(1), 2, 3, PriceType.sell, PriceMeasure.avg);
            Assert.AreEqual("=ImportXML(\"http://api.eve-central.com/api/marketstat?typeid=1&regionlimit=2&usesystem=3&clock=\"&GoogleClock(), \"/evec_api/marketstat/type/sell/avg\")", cell1.CellText(null));
        }
    }

    [TestFixture]
    class InventionBlueprintTests
    {
        [TestCase("Stiletto", null)]
        [TestCase("Sabre", "Formation Layout")]
        public void TestInventionSpreadsheet(string typeName, string decryptorName)
        {
            var bp = new T2Blueprint(CommonQueries.GetBlueprintID(typeName), 2, 0);
            Console.WriteLine("------");
            Console.WriteLine(InventionSpreadsheet.Create<TSVSpreadsheet>(bp, decryptor: Decryptor.Get(decryptorName)).Export());
            Console.WriteLine("------");
        }
    }

    [TestFixture]
    public class ManufacturingSpreadsheetTests
    {
        [TestCase("Large Trimark Armor Pump I")]
        //[TestCase("Warrior II")]
        public void MedTrimark(string typeName)
        {
            var bp = new T1Blueprint(CommonQueries.GetBlueprintID(typeName), 2, 0);

            Console.WriteLine("------");
            Console.WriteLine(ManufacturingSpreadsheet.Create<TSVSpreadsheet>(bp).Export());
            Console.WriteLine("------");
        }
    }
}
