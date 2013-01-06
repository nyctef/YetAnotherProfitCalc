using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc.Tests
{
    [TestFixture]
    public class BlueprintsTests
    {
        [Test]
        public void TestImprovedCloak()
        {
            var t2Cloak = CommonQueries.GetBlueprintID("Improved Cloaking Device II");
            var t2CloakBP = new T2Blueprint(t2Cloak, 0, 5);
            var expected = new[] {
                    new BPMaterial("Morphite", 11),
                    new BPMaterial("Transmitter", 10),
                    new BPMaterial("Miniature Electronics", 5),
                    new BPMaterial("Prototype Cloaking Device I", 1),
                    new BPMaterial("R.A.M.- Electronics", 1, 0.15m),
                    new BPMaterial("Photon Microprocessor", 10),
                    new BPMaterial("Graviton Pulse Generator", 10)
                };
            Assert.That(t2CloakBP.Materials.OrderBy(m => m.matID).SequenceEqual(
                expected.OrderBy(m => m.matID)
            ));
            Console.WriteLine(t2CloakBP.Materials.GetPrice(new BasicEveCentralJitaPriceProvider()));
        }


        [Test]
        public void TestMedTrimark()
        {
            var trimarkID = CommonQueries.GetBlueprintID("Medium Trimark Armor Pump I");
            var trimarkBP = new T1Blueprint(trimarkID, 2, 5);
            var expected = new[] {
                    new BPMaterial("Armor Plates", 13),
                    new BPMaterial("Fried Interface Circuit", 16),
                    new BPMaterial("Contaminated Nanite Compound", 10),
                };
            CollectionAssert.AreEquivalent(expected, trimarkBP.Materials);
            Console.WriteLine(trimarkBP.Materials.GetPrice(new BasicEveCentralJitaPriceProvider()));
        }

        [TestCase("Archon")]
        [TestCase("Aeon")]
        [TestCase("Chimera")]
        [TestCase("Wyvern")]
        [TestCase("Thanatos")]
        [TestCase("Nyx")]
        [TestCase("Nidhoggur")]
        [TestCase("Hel")]
        public void TestMatPercentages(string itemName)
        {
            Console.WriteLine(CommonQueries.GetTypeID("Capital Armor Plates") + "/" + CommonQueries.GetBlueprintFromProduct(CommonQueries.GetMaterialID("Capital Armor Plates")));
            Console.WriteLine(itemName + ": " + CommonQueries.GetTypeID(itemName));
            var bpID = CommonQueries.GetBlueprintID(itemName);
            var bp = new T1Blueprint(bpID, 0, 0);
            var totals = new Dictionary<MaterialID, long>();

            foreach (var capMat in bp.Materials)
            {
                Console.WriteLine(CommonQueries.GetTypeName(capMat.matID) + ":" + capMat.quantity);
                var matBP = new T1Blueprint(CommonQueries.GetBlueprintFromProduct(capMat.matID), 0, 0);
                foreach (var capMatMat in matBP.Materials)
                {
                    if (!totals.ContainsKey(capMatMat.matID))
                        totals.Add(capMatMat.matID, 0);
                    totals[capMatMat.matID] += capMatMat.quantity * capMat.quantity;
                }
            }

            foreach (var key in totals.Keys)
            {
                Console.WriteLine(CommonQueries.GetTypeName(key) + ": " + totals[key]);
            }

            var provider = new BasicEveCentralJitaPriceProvider();
            var prices = new Dictionary<MaterialID, ISK>();
            foreach (var kvp in totals)
            {
                prices.Add(kvp.Key, provider.GetPrice(kvp.Key).ToDecimal() * kvp.Value);
            }

            var total = prices.Aggregate(0m, (current, kvp) => current.ToDecimal() + kvp.Value.ToDecimal());

            foreach (var kvp in prices)
            {
                Console.WriteLine(CommonQueries.GetTypeName(kvp.Key) + ": " + 100 * (kvp.Value.ToDecimal() / total));
            }
        }

        [Test]
        public void TestMedCargoRig()
        {
            var rig = CommonQueries.GetBlueprintID("Medium Cargohold Optimization I");
            var t1Blueprint = new T1Blueprint(rig, 2, 5);
            var result = t1Blueprint.Materials.GetPrice(new BasicEveCentralJitaPriceProvider());
            foreach (var mat in t1Blueprint.Materials)
            {
                Console.WriteLine("Needs " + mat.quantity + " of " + mat.Name);
            }
            Console.WriteLine(result);
        }

        [TestCase("Medium Shield Extender I")]
        [TestCase("Warrior I")]
        public void TestInventionProfit(string typeName)
        {
            var priceProvider = new BasicPriceCache(new BasicEveCentralJitaPriceProvider());
            var matID = CommonQueries.GetMaterialID(typeName);
            var bp = new T1Blueprint(CommonQueries.GetBlueprintFromProduct(matID), 0, 0);
            var t2Cost = bp.GetT2BpcCost(priceProvider, 4, 4, 4);
            Console.WriteLine("inventionCost: " + t2Cost.Format());
            int t2runs;
            var t2bp = bp.GetInventionResult(out t2runs);
            var t2ProductCost = priceProvider.GetPrice(t2bp.Product).ToDecimal();
            Console.WriteLine("t2ProductCost: " + t2ProductCost.FormatISK());
            var t2MatsCost = t2bp.Materials.GetPrice(priceProvider).ToDecimal();
            Console.WriteLine("t2MatsCost: " + t2MatsCost.FormatISK());
            var t2Revenue = t2ProductCost - t2MatsCost;
            var profit = t2Revenue.ToDecimal() * t2runs - t2Cost.ToDecimal();
            Console.WriteLine("profit: " + profit.FormatISK());
            Console.WriteLine("copying time: " + (bp.CopyTime()).FormatSeconds());
            Console.WriteLine("invention time: " + (bp.InventionTime() / bp.InventionChance(4, 4, 4)).FormatSeconds());
            Console.WriteLine("t2 manufacture time: " + (t2bp.ManufacturingTime() * t2runs).FormatSeconds());
        }

        [TestCase(" Drake\t1 Battlecruiser")]
        public void TestContractValue(string contractPaste)
        {
            var mats = new Dictionary<MaterialID, int>();
            var reg = new Regex(@"([a-zA-Z\d'\- ]*)\t\d+\.*");
            foreach (var line in contractPaste.Split(new[] { '\n' }))
            {
                if (String.IsNullOrWhiteSpace(line)) continue;
                var match = reg.Match(line).Groups[1];
                mats.AddToCount(CommonQueries.GetMaterialID(match.Value.Trim()));
            }

            var provider = new BasicEveCentralDelvePriceProvider();
            var prices = new Dictionary<MaterialID, ISK>();
            foreach (var kvp in mats.OrderBy(kvp => CommonQueries.GetTypeName(kvp.Key)))
            {
                prices.Add(kvp.Key, provider.GetPrice(kvp.Key).ToDecimal() * kvp.Value);
                Console.WriteLine(CommonQueries.GetTypeName(kvp.Key) + " x" + kvp.Value + " : " + prices[kvp.Key].Format());
            }

            var total = prices.Aggregate(0m, (current, kvp) => current.ToDecimal() + kvp.Value.ToDecimal());

            Console.WriteLine("total: " + total.FormatISK());
        }

        [Test]
        public void CapComponentsProfit()
        {
            var priceProvider = new BasicPriceCache(new BasicEveCentralJitaPriceProvider());
            var bpTypes = CommonQueries.GetTypesWithNamesLike("Capital % Blueprint").Select(tID => new BlueprintID(tID.Item2.ToInt()));
            foreach (var bpType in bpTypes)
            {
                var bp = new T1Blueprint(bpType, 5, 5);
                Console.WriteLine(CommonQueries.GetTypeName(bp.Product) + " Mats: ");
                var totalPrice = 0m;
                foreach (var mat in bp.Materials)
                {
                    var price = priceProvider.GetPrice(mat.matID).ToDecimal() * mat.quantity * mat.damagePerJob;
                    totalPrice += price;
                    Console.WriteLine(CommonQueries.GetTypeName(mat.matID) + ": " + price.FormatISK());
                }
                Console.WriteLine();
                Console.WriteLine("Total price: " + totalPrice.FormatISK());
                var productPrice = priceProvider.GetPrice(bp.Product);
                Console.WriteLine("Product price: " + productPrice);
                var profit = productPrice.ToDecimal() - totalPrice;
                Console.WriteLine("Profit: " + profit.FormatISK());
            }
        }

        [Test]
        public void RigsToMake()
        {
            var prices = new BasicPriceCache(new BasicEveCentralJitaPriceProvider());

            var groupID = GroupID.RigBlueprint;

            var results = new Dictionary<string, Tuple<decimal, ISK, ISK, ISK>>();

            var blueprintsQuery = @"select typeID from invTypes where groupID = "+groupID+" and published = 1;";
            using (var conn = new SQLiteConnection(CommonQueries.DefaultDatabase.dbConnection))
            {
                conn.Open();
                var reader = CommonQueries.DefaultDatabase.RunSQLTableQuery(blueprintsQuery, conn);
                while (reader.Read())
                {
                    var bpId = new BlueprintID(reader["typeID"].ToInt());
                    var bp = new T1Blueprint(bpId, 2, 0);
                    var name = CommonQueries.GetTypeName(bp.Product);
                    if (name.Contains("II")) continue;

                    Console.WriteLine(name);
                    var cost = bp.Materials.GetPrice(prices);
                    var price = prices.GetPrice(bp.Product);
                    var profit = new ISK(price.ToDecimal() - cost.ToDecimal());
                    var markup = profit.ToDecimal() / cost.ToDecimal();

                    results.Add(name, new Tuple<decimal, ISK, ISK, ISK>(markup, cost, price, profit));

                    //Console.WriteLine("Cost:   " + cost);
                    //Console.WriteLine("Price:  " + price);
                    //Console.WriteLine("Profit: " + profit);
                    //Console.WriteLine("Markup: " + markup*100);
                    //Console.WriteLine();

                    Thread.Sleep(50);
                }
            }

            // results
            foreach (var result in 
                results
                .OrderByDescending(result => result.Value.Item1)
                .Where(result => result.Value.Item4.ToDecimal() > 100*1000)
                .Take(10))
            {
                Console.WriteLine(result.Key);
                Console.WriteLine("Cost:   " + result.Value.Item2);
                Console.WriteLine("Price:  " + result.Value.Item3);
                Console.WriteLine("Profit: " + result.Value.Item4);
                Console.WriteLine("Markup: " + result.Value.Item1 * 100);
                Console.WriteLine();
            }

            // results
            foreach (var result in
                results
                .OrderByDescending(result => result.Value.Item4)
                .Where(result => result.Value.Item1 > 0.1m)
                .Take(10))
            {
                Console.WriteLine(result.Key);
                Console.WriteLine("Cost:   " + result.Value.Item2);
                Console.WriteLine("Price:  " + result.Value.Item3);
                Console.WriteLine("Profit: " + result.Value.Item4);
                Console.WriteLine("Markup: " + result.Value.Item1 * 100);
                Console.WriteLine();
            }

        }
    }
}
