using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc
{
    class InventionSpreadsheet
    {
        public static TSpreadsheet Create<TSpreadsheet>(T2Blueprint bp, int rowNum = 0, TSpreadsheet existing = null, Decryptor decryptor = null) where TSpreadsheet : class, Spreadsheet, new()
        {
            var spreadsheet = existing ?? new TSpreadsheet();

            spreadsheet.AddCell(new SimpleCell("Invention"), 0, rowNum);

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

            var t1BP = new T1Blueprint(CommonQueries.GetBlueprintFromProduct(CommonQueries.GetT1VersionOfT2(bp.Product)), 0, 0);

            // TODO: too much copy&paste between this and ManufacturingSpreadsheet
            // TODO: brokerFee

            rowNum++;
            Cell topUnitCost = null, bottomUnitCost = null;
            Cell topCostPerRun = null, bottomCostPerRun = null;
            Cell topTotalCost = null, bottomTotalCost = null;
            var mats = t1BP.InventionMaterials.Union(decryptor != null ? new[] { new BPMaterial(decryptor.MaterialID, 1) } : Enumerable.Empty<BPMaterial>());
            foreach (var mat in mats)
            {
                spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(mat.matID)), 0, rowNum);
                var matQty = spreadsheet.AddCell(new SimpleCell("=" + mat.quantity.ToString() + "*" + mat.damagePerJob.ToString()), 1, rowNum);
                var buy = spreadsheet.AddCell(new EveCentralCell(mat.matID, type: PriceType.buy, measure: PriceMeasure.max), 2, rowNum);
                var sell = spreadsheet.AddCell(new EveCentralCell(mat.matID, type: PriceType.sell, measure: PriceMeasure.min), 3, rowNum);
                var bfAmount = spreadsheet.AddCell(new FormulaCell("={0}*0.01", /*brokerFee, */ buy), 4, rowNum);
                // 5 empty
                var buysell = spreadsheet.AddCell(new SimpleCell("Sell"), 6, rowNum);

                var unitCost = spreadsheet.AddCell(new FormulaCell("=if({0}=\"Sell\",{1},if({0}=\"Buy\",{2}+{3},{0}+{3}))", buysell, sell, buy, /*brokerFee*/bfAmount), 7, rowNum);
                bottomUnitCost = unitCost; topUnitCost = topUnitCost ?? unitCost;

                var costPerRun = spreadsheet.AddCell(new FormulaCell("={0}*{1}", unitCost, matQty), 8, rowNum);
                bottomCostPerRun = costPerRun; topCostPerRun = topCostPerRun ?? bottomCostPerRun;

                var totalCost = spreadsheet.AddCell(new FormulaCell("={0}", costPerRun), 9, rowNum);
                bottomTotalCost = totalCost; topTotalCost = topTotalCost ?? totalCost;

                rowNum++;
            }

            spreadsheet.AddCell(new SimpleCell(CommonQueries.GetTypeName(t1BP.InventionInterface)), 0, rowNum);
            spreadsheet.AddCell(new SimpleCell("0"), 1, rowNum);
            spreadsheet.AddCell(new EveCentralCell(t1BP.InventionInterface, type: PriceType.buy, measure: PriceMeasure.max), 2, rowNum);
            spreadsheet.AddCell(new EveCentralCell(t1BP.InventionInterface, type: PriceType.sell, measure: PriceMeasure.min), 3, rowNum);

                       // todo:  meta items
            
            rowNum++;

            var totalUnitCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topUnitCost, bottomUnitCost), 7, rowNum);
            var totalCostPerRun = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topCostPerRun, bottomCostPerRun), 8, rowNum);
            var totalTotalCost = spreadsheet.AddCell(new FormulaCell("=SUM({0}:{1})", topTotalCost, bottomTotalCost), 9, rowNum);

            rowNum +=2;

            spreadsheet.AddCell(new SimpleCell("Datacore 1 skill"), 0, rowNum);
            var d1Skill = spreadsheet.AddCell(new SimpleCell("4"), 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Datacore 2 skill"), 0, rowNum);
            var d2Skill = spreadsheet.AddCell(new SimpleCell("4"), 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Interface skill"), 0, rowNum);
            var intSkill = spreadsheet.AddCell(new SimpleCell("4"), 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Decryptor modifier"), 0, rowNum);
            var decMod = spreadsheet.AddCell(new SimpleCell(decryptor != null ? decryptor.InventionMod.ToString() : "1"), 1, rowNum);
            rowNum++;
            spreadsheet.AddCell(new SimpleCell("Invention chance"), 0, rowNum);
            var invChance = spreadsheet.AddCell(new FormulaCell("="+t1BP.BaseInventionChance()+"*(1+0.01*{0})*(1+({1}+{2})*(0.1/5-0))*{3}", intSkill, d1Skill, d2Skill, decMod), 1, rowNum);
            rowNum++;
            
            spreadsheet.AddCell(new SimpleCell("Cost per BP"), 0, rowNum);
            var costPerBP = spreadsheet.AddCell(new FormulaCell("={0}/{1}", totalTotalCost, invChance), 1, rowNum);
            rowNum++;

            int outputruns;
            var invResult = decryptor != null ? t1BP.GetInventionResult(out outputruns, MLmod:decryptor.MlResult, PLmod:decryptor.PlResult, runsMod:decryptor.RunsMod, outputName:CommonQueries.GetTypeName(bp.Product))
                                              : t1BP.GetInventionResult(out outputruns, outputName: CommonQueries.GetTypeName(bp.Product));

            spreadsheet.AddCell(new SimpleCell("Cost per unit"), 0, rowNum);
            var costPerUnit = spreadsheet.AddCell(new FormulaCell("={0}/"+outputruns, costPerBP), 1, rowNum);
            rowNum += 2;

            ManufacturingSpreadsheet.Create<TSpreadsheet>(invResult, rowNum, spreadsheet, costPerUnit);

            rowNum = spreadsheet.Height + 1;
            spreadsheet.AddCell(new SimpleCell("Copying time p/u: "), 0, rowNum);
            spreadsheet.AddCell(new SimpleCell((t1BP.CopyTime()/outputruns).FormatSeconds()), 1, rowNum);
            spreadsheet.AddCell(new SimpleCell((t1BP.CopyTime()/outputruns).ToString()), 2, rowNum);
            rowNum++;

            spreadsheet.AddCell(new SimpleCell("Invention time p/u: "), 0, rowNum);
            spreadsheet.AddCell(new SimpleCell((t1BP.InventionTime()/outputruns).FormatSeconds()), 1, rowNum);
            spreadsheet.AddCell(new SimpleCell((t1BP.InventionTime()/outputruns).ToString()), 2, rowNum);
            
            return spreadsheet;
        }
    }

    
}
