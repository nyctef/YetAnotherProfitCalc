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

        public static string Create(IBlueprint bp)
        {
            var spreadsheet = new TSVSpreadsheet();

            spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(bp.Product)), 0, 0);
            var quantity = spreadsheet.AddCell(new SimpleCell("100"), 0, 1);
            var brokerFee = spreadsheet.AddCell(new SimpleCell("0.0045"), 1, 0);
            var transactionTax = spreadsheet.AddCell(new SimpleCell("0.0105"), 2, 0);

            spreadsheet.AddCell(new SimpleCell("Quantity"), 1, 1);
            spreadsheet.AddCell(new SimpleCell("Buy"), 2, 1);
            spreadsheet.AddCell(new SimpleCell("Sell"), 3, 1);
            spreadsheet.AddCell(new SimpleCell("BrokerFee"), 4, 1);
            spreadsheet.AddCell(new SimpleCell("TransactionTax"), 5, 1);
            spreadsheet.AddCell(new SimpleCell("Buy/Sell/CustomBuy"), 6, 1);
            spreadsheet.AddCell(new SimpleCell("Unit Cost"), 7, 1);
            spreadsheet.AddCell(new SimpleCell("Cost per run"), 8, 1);
            spreadsheet.AddCell(new SimpleCell("Total cost"), 9, 1);
            spreadsheet.AddCell(new SimpleCell("(Needed)"), 10, 1);

            var rowOffset = 2;
            Cell topUnitCost = null, bottomUnitCost = null;
            Cell topCostPerRun = null, bottomCostPerRun = null;
            Cell topTotalCost = null, bottomTotalCost = null;
            foreach (var mat in bp.Materials)
            {
                spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(mat.matID)), 0, rowOffset);
                var matQty = spreadsheet.AddCell(new SimpleCell(mat.quantity.ToString()), 1, rowOffset);
                var buy = spreadsheet.AddCell(new EveCentralCell(mat.matID, type:PriceType.buy, measure:PriceMeasure.max), 2, rowOffset);
                var sell = spreadsheet.AddCell(new EveCentralCell(mat.matID, type: PriceType.sell, measure: PriceMeasure.min), 3, rowOffset);
                var bfAmount = spreadsheet.AddCell(new FormulaCell("={0}*{1}", brokerFee, buy), 4, rowOffset);
                // 5 empty
                var buysell = spreadsheet.AddCell(new SimpleCell("Sell"), 6, rowOffset);

                var unitCost = spreadsheet.AddCell(new FormulaCell("=if({0}=\"Sell\",{1},if({0}=\"Buy\",{2}+{3},{0}+{3}))", buysell, sell, buy, brokerFee), 7, rowOffset);
                bottomUnitCost = unitCost; topUnitCost = topUnitCost ?? unitCost;

                var costPerRun = spreadsheet.AddCell(new FormulaCell("={0}*{1}", unitCost, matQty), 8, rowOffset);
                bottomCostPerRun = costPerRun; topCostPerRun = topCostPerRun ?? bottomCostPerRun;

                var totalCost = spreadsheet.AddCell(new FormulaCell("={0}*{1}", costPerRun, quantity), 9, rowOffset);
                bottomTotalCost = totalCost; topTotalCost = topTotalCost ?? totalCost;

                var needed = spreadsheet.AddCell(new FormulaCell("={0}*{1}", matQty, quantity), 10, rowOffset);

                rowOffset++;
            }

            var totalUnitCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topUnitCost, bottomUnitCost), 7, rowOffset);
            var totalCostPerRun = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topCostPerRun, bottomCostPerRun), 8, rowOffset);
            var totalTotalCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topTotalCost, bottomTotalCost), 9, rowOffset);

            rowOffset += 2;

            spreadsheet.AddCell(new SimpleCell("Buy/Sell/CustomSell"), 6, rowOffset);
            spreadsheet.AddCell(new SimpleCell("Unit Value"), 7, rowOffset);
            spreadsheet.AddCell(new SimpleCell("Total Value"), 9, rowOffset);

            rowOffset++;
            spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(bp.Product)), 0, rowOffset);
            var prodBuy = spreadsheet.AddCell(new EveCentralCell(bp.Product, type: PriceType.buy, measure: PriceMeasure.max), 2, rowOffset);
            var prodSell = spreadsheet.AddCell(new EveCentralCell(bp.Product, type: PriceType.sell, measure: PriceMeasure.min), 3, rowOffset);
            // 4 empty
            var ttAmount = spreadsheet.AddCell(new FormulaCell("={0}*{1}", prodSell, transactionTax), 5, rowOffset);
            var prodBuySell = spreadsheet.AddCell(new SimpleCell("Sell"), 6, rowOffset);
            var prodValue = spreadsheet.AddCell(new FormulaCell("=if({0}=\"Sell\",{1},if({0}=\"Buy\",{2}-{3},{0}-{3}))", prodBuySell, prodSell, prodBuy, ttAmount), 7, rowOffset);
            var totalValue = spreadsheet.AddCell(new FormulaCell("={0}*{1}", prodValue, quantity), 9, rowOffset);

            rowOffset += 2;
            spreadsheet.AddCell(new SimpleCell("Profit"), 9, rowOffset);
            spreadsheet.AddCell(new SimpleCell("Margin"), 10, rowOffset);

            rowOffset++;
            var profit = spreadsheet.AddCell(new FormulaCell("={0}-{1}", totalValue, totalTotalCost), 9, rowOffset);
            spreadsheet.AddCell(new FormulaCell("={0}/{1}", profit, totalValue), 10, rowOffset);
            
            return spreadsheet.Export();
        }

    }

    public class ManufacturingSpreadsheetTests
    {
        [TestCase("Medium Trimark Armor Pump I")]
        public void MedTrimark(string typeName)
        {
            var bp = new T1Blueprint(CommonQueries.GetBlueprintID(typeName), 2, 0);
            Console.WriteLine("------");
            Console.WriteLine(ManufacturingSpreadsheet.Create(bp));
            Console.WriteLine("------");
        }
    }
}
