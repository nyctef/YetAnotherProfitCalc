using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc
{
    class ManufacturingSpreadsheet
    {

        public static TSpreadsheet Create<TSpreadsheet>(IBlueprint bp, int rowNum = 0, TSpreadsheet existing = null, Cell blueprintCost = null, int numToMake = 1) where TSpreadsheet : class, Spreadsheet, new()
        {
            var spreadsheet = existing ?? new TSpreadsheet();

            spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(bp.Product)), 0, rowNum);
            var quantity = new SimpleCell(numToMake.ToString());

            var brokerFee = new SimpleCell("0.0045");
            var transactionTax = new SimpleCell("0.0105");
            rowNum++;

            spreadsheet.AddCell(new SimpleCell("Quantity"), 1, rowNum);
            spreadsheet.AddCell(new SimpleCell("Buy"), 2, rowNum);
            spreadsheet.AddCell(new SimpleCell("Sell"), 3, rowNum);
            spreadsheet.AddCell(new SimpleCell("BrokerFee"), 4, rowNum);
            spreadsheet.AddCell(new SimpleCell("TransactionTax"), 5, rowNum);
            spreadsheet.AddCell(new SimpleCell("Buy/Sell/CustomBuy"), 6, rowNum);
            spreadsheet.AddCell(new SimpleCell("Unit Cost"), 7, rowNum);
            spreadsheet.AddCell(new SimpleCell("Cost per run"), 8, rowNum);
            spreadsheet.AddCell(new SimpleCell("Total cost"), 9, rowNum);
            spreadsheet.AddCell(new SimpleCell("(Needed)"), 10, rowNum);

            rowNum++;
            Cell topUnitCost = null, bottomUnitCost = null;
            Cell topCostPerRun = null, bottomCostPerRun = null;
            Cell topTotalCost = null, bottomTotalCost = null;
            foreach (var mat in bp.Materials)
            {
                spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(mat.matID)), 0, rowNum);
                var matQty = spreadsheet.AddCell(new SimpleCell("=" + mat.quantity.ToString() + "*" + mat.damagePerJob.ToString()), 1, rowNum);

                var buy = spreadsheet.AddCell(new EveCentralCell(mat.matID, type:PriceType.buy, measure:PriceMeasure.max), 2, rowNum);
                var sell = spreadsheet.AddCell(new EveCentralCell(mat.matID, type: PriceType.sell, measure: PriceMeasure.min), 3, rowNum);
                var bfAmount = spreadsheet.AddCell(new FormulaCell("={0}*{1}", brokerFee, buy), 4, rowNum);
                // 5 empty
                var buysell = spreadsheet.AddCell(new SimpleCell("Sell"), 6, rowNum);

                var unitCost = spreadsheet.AddCell(new FormulaCell("=if({0}=\"Sell\",{1},if({0}=\"Buy\",{2}+{3},{0}+{3}))", buysell, sell, buy, bfAmount), 7, rowNum);

                bottomUnitCost = unitCost; topUnitCost = topUnitCost ?? unitCost;

                var costPerRun = spreadsheet.AddCell(new FormulaCell("={0}*{1}", unitCost, matQty), 8, rowNum);
                bottomCostPerRun = costPerRun; topCostPerRun = topCostPerRun ?? bottomCostPerRun;

                var totalCost = spreadsheet.AddCell(new FormulaCell("={0}*{1}", costPerRun, quantity), 9, rowNum);
                bottomTotalCost = totalCost; topTotalCost = topTotalCost ?? totalCost;

                var needed = spreadsheet.AddCell(new FormulaCell("={0}*{1}", matQty, quantity), 10, rowNum);

                rowNum++;
            }

            if (blueprintCost != null)
            {
                spreadsheet.AddCell(new SimpleCell("Blueprint (per unit)"), 0, rowNum);
                var costPerRun = spreadsheet.AddCell(new FormulaCell("={0}", blueprintCost), 8, rowNum);
                bottomCostPerRun = costPerRun; topCostPerRun = topCostPerRun ?? bottomCostPerRun;

                var totalCost = spreadsheet.AddCell(new FormulaCell("={0}*{1}", costPerRun, quantity), 9, rowNum);
                bottomTotalCost = totalCost; topTotalCost = topTotalCost ?? totalCost;

                rowNum++;
            }

            var totalUnitCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topUnitCost, bottomUnitCost), 7, rowNum);
            var totalCostPerRun = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topCostPerRun, bottomCostPerRun), 8, rowNum);
            var totalTotalCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topTotalCost, bottomTotalCost), 9, rowNum);

            rowNum += 2;

            spreadsheet.AddCell(new SimpleCell("Buy/Sell/CustomSell"), 6, rowNum);
            spreadsheet.AddCell(new SimpleCell("Unit Value"), 7, rowNum);
            spreadsheet.AddCell(new SimpleCell("Total Value"), 9, rowNum);

            rowNum++;
            spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(bp.Product)), 0, rowNum);
            var prodBuy = spreadsheet.AddCell(new EveCentralCell(bp.Product, type: PriceType.buy, measure: PriceMeasure.max), 2, rowNum);
            var prodSell = spreadsheet.AddCell(new EveCentralCell(bp.Product, type: PriceType.sell, measure: PriceMeasure.min), 3, rowNum);
            // 4 empty
            var ttAmount = spreadsheet.AddCell(new FormulaCell("={0}*{1}", prodSell, transactionTax), 5, rowNum);
            var prodBuySell = spreadsheet.AddCell(new SimpleCell("Sell"), 6, rowNum);
            var prodValue = spreadsheet.AddCell(new FormulaCell("=if({0}=\"Sell\",{1}-{3},if({0}=\"Buy\",{2},{0}-{3}))", prodBuySell, prodSell, prodBuy, ttAmount), 7, rowNum);
            var totalValue = spreadsheet.AddCell(new FormulaCell("={0}*{1}", prodValue, quantity), 9, rowNum);

            rowNum += 2;
            spreadsheet.AddCell(new SimpleCell("Profit"), 9, rowNum);
            spreadsheet.AddCell(new SimpleCell("Margin"), 10, rowNum);

            rowNum++;
            var profit = spreadsheet.AddCell(new FormulaCell("={0}-{1}", totalValue, totalTotalCost), 9, rowNum);
            spreadsheet.AddCell(new FormulaCell("={0}/{1}", profit, totalValue), 10, rowNum);

            rowNum--;

            spreadsheet.AddCell(new SimpleCell("Number of runs: "), 0, rowNum);
            spreadsheet.AddCell(quantity, 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Broker fee: "), 0, rowNum);
            spreadsheet.AddCell(brokerFee, 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Transaction tax: "), 0, rowNum);
            spreadsheet.AddCell(transactionTax, 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Production time: "), 0, rowNum);
            spreadsheet.AddCell(new SimpleCell(bp.ManufacturingTime().FormatSeconds()), 1, rowNum);
            var mt = spreadsheet.AddCell(new SimpleCell(bp.ManufacturingTime().ToString()), 2, rowNum);

            return spreadsheet;
        }

    }

    public class ManufacturingSpreadsheetTests
    {
        [TestCase("Medium Trimark Armor Pump I")]
        [TestCase("Warrior II")]
        public void MedTrimark(string typeName)
        {
            var bp = new T1Blueprint(CommonQueries.GetBlueprintID(typeName), 2, 0);

			Console.WriteLine("------");
            Console.WriteLine(ManufacturingSpreadsheet.Create<TSVSpreadsheet>(bp).Export());
            Console.WriteLine("------");
        }


    }
}
