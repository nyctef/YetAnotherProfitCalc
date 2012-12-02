using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Data.SQLite;
=======
>>>>>>> 525a2ba2ec4cc2beae08a2fba3fc2e0a3c50f35e
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YetAnotherProfitCalc.Enumerations;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace YetAnotherProfitCalc
{
	class BPMaterial : IEquatable<BPMaterial>
	{
		public readonly MaterialID matID;
		public readonly long quantity;
		public readonly decimal damagePerJob;

		public BPMaterial(MaterialID matID, long quantity, decimal damagePerJob = 1)
		{
			this.matID = matID;
			this.quantity = quantity;
			this.damagePerJob = damagePerJob;
		}

		public BPMaterial(string typeName, long quantity, decimal damagePerJob = 1)
			: this(new MaterialID(CommonQueries.GetTypeID(typeName).ToInt()), quantity, damagePerJob) { }

		public string Name { get { return CommonQueries.GetTypeName(matID); } }

		public override bool Equals(object obj)
		{
			if (!(obj is BPMaterial)) return false;
			return Equals((BPMaterial)obj);
		}

		public bool Equals(BPMaterial other)
		{
			return this.matID.Equals(other.matID) &&
				this.quantity == other.quantity &&
				this.damagePerJob == other.damagePerJob;
		}

		public override int GetHashCode()
		{
			return matID.GetHashCode() + 19 * quantity.GetHashCode() + damagePerJob.GetHashCode();
		}

		public override string ToString()
		{
			return Name + "/" + quantity + (damagePerJob > 0 ? "/" + damagePerJob : "");
		}
	}

	interface IBlueprint
	{
		int MatLevel { get; }
		int ProdLevel { get; }
		MaterialID Product { get; }
		List<BPMaterial> Materials { get; }
		BPDetails Details { get; }
	}

	public class BPDetails
	{
		public readonly int productionTime;
		public readonly int researchProductivityTime;
		public readonly int researchMaterialTime;
		public readonly int researchCopyTime;
		public readonly int researchTechTime;
		public readonly int productivityModifier;
		public readonly int materialModifier;
		public readonly int wasteFactor;
		public readonly int maxProductionLimit;

		public BPDetails(int productionTime, int researchProductivityTime, int researchMaterialTime, int researchCopyTime, int researchTechTime, int productivityModifier, int materialModifier, int wasteFactor, int maxProductionLimit)
		{
			this.productionTime = productionTime;
			this.researchProductivityTime = researchProductivityTime;
			this.researchMaterialTime = researchMaterialTime;
			this.researchCopyTime = researchCopyTime;
			this.researchTechTime = researchTechTime;
			this.productivityModifier = productivityModifier;
			this.materialModifier = materialModifier;
			this.wasteFactor = wasteFactor;
			this.maxProductionLimit = maxProductionLimit;
		}
	}

	class T1Blueprint : IBlueprint
	{
		public int MatLevel { get; private set; }
		public int ProdLevel { get; private set; }
		public MaterialID Product { get; private set; }
		public List<BPMaterial> Materials { get; private set; }
		public List<BPMaterial> InventionMaterials { get; private set; }
		public MaterialID InventionInterface { get; private set; }
		public BPDetails Details { get; private set; }

		public T1Blueprint(BlueprintID bpID, int matlevel, int prodLevel)
		{
			MatLevel = matlevel;
			ProdLevel = prodLevel;
            Product = CommonQueries.GetProductFromBlueprint(bpID);
            Details = CommonQueries.GetBlueprintDetails(bpID);

			Materials = CommonQueries.GetMaterialsRaw(Product)
                .AdjustForMEWastage(matlevel, Details.wasteFactor)
                .Union(CommonQueries.GetMaterialsExtra(bpID))
                .ToList();

			var extraMats = CommonQueries.GetMaterialsExtra(bpID, ActivityIDs.Invention).ToList();
            if (extraMats.Any())
            {
                // data dump is terrible and gives data interfaces a damagePerJob of 100% :ccp:
                InventionMaterials = extraMats
                    .Where(m => m.matID.GetGroupID() != GroupID.DataInterfaces) 
                    .ToList();
                InventionInterface = extraMats
                    .Single(m => m.matID.GetGroupID() == GroupID.DataInterfaces)
                    .matID;
            }
        }

        #region detail methods

        public bool HasT2Version() 
        {
            return CommonQueries.GetT2VersionOfT1(Product).ToInt() != 0;
        }

        public decimal InventionChance(int encryptionSkillLevel, int datacore1SkillLevel, int datacore2SkillLevel, int itemMetaLevel = 0) 
        {
            return BaseInventionChance() * (1 + 0.01m*encryptionSkillLevel) * (1+(datacore1SkillLevel + datacore2SkillLevel) *(0.1m/(5-itemMetaLevel)));
        }

        public decimal BaseInventionChance() 
        {
            var typeID = Product;
            var groupID = CommonQueries.GetGroupID(Product);

            if (groupID == GroupID.Battlecruiser || groupID == GroupID.Battleship ||
                typeID == CommonQueries.GetTypeID("Covetor")) 
            {
                return 0.2m;
            }

            if (groupID == GroupID.Cruiser || groupID == GroupID.Industrial ||
                typeID == CommonQueries.GetTypeID("Retriever")) 
            {
                return 0.25m;
            }

            if (groupID == GroupID.Frigate || groupID == GroupID.Destroyer ||
                groupID == GroupID.Freighter || typeID == CommonQueries.GetTypeID("Procurer"))
            {
                return 0.3m;
            }

            if (HasT2Version()) 
            {
                return 0.4m;
            }

            return 0;
        }

        public decimal MEResearchTime(int metallurgySkillLevel, decimal researchSlotModifier = 1, decimal implantModifier = 1) 
        {
            return Details.researchMaterialTime * (1 - (0.05m * metallurgySkillLevel)) * researchSlotModifier * implantModifier;
        }

        public decimal PEResearchTime(int researchSkillLevel, decimal researchSlotModifier = 1, decimal implantModifier = 1)
        {
            return Details.researchProductivityTime * (1 - (0.05m * researchSkillLevel)) * researchSlotModifier * implantModifier;
        }

        /// <summary>
        /// Copy time for a max-run copy
        /// </summary>
        public decimal CopyTime(int scienceSkillLevel = 5, decimal copySlotModifier = 1, decimal implantModifier = 1)
        {
            // From wiki.eve-id.net: "Note that [researchCopyTime] is the amount of time taken to copy a number of runs equal to 
            // half the maxProductionLimit, whether as multiple runs on one copy or as one run each on multiple copies." :ccp:
            return Details.researchCopyTime * 2 * (1 - (0.04m * scienceSkillLevel)) * copySlotModifier * implantModifier;
        }
             
        public decimal InventionTime(int inventionSlotModifier = 1, int implantModifier = 1)
        {
            return Details.researchTechTime * inventionSlotModifier * implantModifier;
        }

        public T2Blueprint GetInventionResult(out int outputRuns, int inputRuns = -1, int MLmod = -4, int PLmod = -4, int runsMod = 0)
        {
            if (inputRuns == -1) inputRuns = Details.maxProductionLimit;
            var t2bpID = CommonQueries.GetBlueprintFromProduct(CommonQueries.GetT2VersionOfT1(Product));
            var t2bp = new T2Blueprint(t2bpID, MLmod, PLmod);
            outputRuns = (int)((inputRuns / (decimal)Details.maxProductionLimit) * (t2bp.Details.maxProductionLimit / 10.0m)) + runsMod;
            outputRuns = Math.Min(Math.Max(outputRuns, 1), t2bp.Details.maxProductionLimit);
            return t2bp;
        }

        #endregion
        
	}
	

	class T2Blueprint : IBlueprint
	{
		public int MatLevel { get; private set; }
		public int ProdLevel { get; private set; }
		public MaterialID Product { get; private set; }
		public List<BPMaterial> Materials { get; private set; }
		public BPDetails Details { get; private set; }

		public T2Blueprint(BlueprintID bpID, int matLevel, int prodLevel)
		{
			MatLevel = matLevel;
			ProdLevel = prodLevel;
			Product = CommonQueries.GetProductFromBlueprint(bpID);
			Details = CommonQueries.GetBlueprintDetails(bpID);

			var t1ID = CommonQueries.GetT1VersionOfT2(Product);
			var t1BPID = CommonQueries.GetBlueprintFromProduct(t1ID);
			//Console.WriteLine(new T1Blueprint(t1BPID, 0, 0).InventionMaterials);

			Materials = new List<BPMaterial>();

			var rawMaterials = CommonQueries.GetMaterialsRaw(Product);
			var extraMaterials = CommonQueries.GetMaterialsExtra(bpID);
			var t1Materials = CommonQueries.GetMaterialsRaw(t1ID);
			var extraT1Materials = CommonQueries.GetMaterialsExtra(t1BPID);
			// TODO: currently this assumes there is always exactly 1 recyclable material which is the t1 item
			var rawMaterialsAfterRecycling = rawMaterials.Subtract(t1Materials);
			// adjust for ME
            var adjustedRawMaterials = rawMaterialsAfterRecycling.AdjustForMEWastage(matLevel, Details.wasteFactor);

			Materials = adjustedRawMaterials.Union(extraMaterials).ToList();
		}
	}

	static class BlueprintExtensions
	{
		public static List<BPMaterial> Subtract(this IEnumerable<BPMaterial> baseMats, IEnumerable<BPMaterial> toSubtract)
		{
			var toSubDic = toSubtract.ToDictionary(m => m.matID);
			var result = new List<BPMaterial>();

			foreach (var baseType in baseMats)
			{
				var newQuantity = toSubDic.ContainsKey(baseType.matID) ? baseType.quantity - toSubDic[baseType.matID].quantity
                                                                       : baseType.quantity;
				Debug.Assert(newQuantity >= 0);
				if (newQuantity > 0)
				{
					result.Add(new BPMaterial(baseType.matID, newQuantity));
				}
			}

			return result;
		}

        public static List<BPMaterial> AdjustForMEWastage(this IEnumerable<BPMaterial> mats, int ME, int wasteFactor = 10)
        {
            var meWastageFactor = 1 + CommonQueries.GetWasteForME(ME, wasteFactor);
            return mats.Select(m => new BPMaterial(m.matID, (long)(m.quantity * meWastageFactor))).ToList();
        }

		public static ISK GetPrice(/* [NotNull] */ this IEnumerable<BPMaterial> materials, /* [NotNull] */ IPriceProvider priceProvider)
		{
			decimal result = 0m;
			foreach (BPMaterial material in materials)
			{
				result = result + material.GetPrice(priceProvider).ToDecimal();
			}
			return result;
		}

		public static ISK GetPrice(this BPMaterial mat, IPriceProvider priceProvider)
		{
			var price = priceProvider.GetPrice(mat.matID).ToDecimal();
			return price * mat.quantity * mat.damagePerJob;
		}

        public static ISK GetT2BpcCost(this T1Blueprint bp, IPriceProvider priceProvider, int encryptorSkillLevel, int datacore1SkillLevel, int datacore2SkillLevel, int addedItemMetaLevel = 0, decimal decryptorModifier = 1)
        {
            if (!bp.HasT2Version()) throw new ArgumentException("bp", "blueprint must have a t2 version");
            var singleTryCost = bp.InventionMaterials.GetPrice(priceProvider);
            decimal inventionChance = bp.InventionChance(encryptorSkillLevel, datacore1SkillLevel, datacore2SkillLevel, addedItemMetaLevel);
            return singleTryCost.ToDecimal() / inventionChance;
        }

        public static decimal ManufacturingTime(this IBlueprint bp, int industrySkill = 5, decimal implantModifier = 1, decimal productionSlotModifier = 1)
        {
            decimal productionTimeModifier = (1 - (0.04m * industrySkill)) * implantModifier * productionSlotModifier;
            decimal PEFactor = bp.ProdLevel < 0 ? (bp.ProdLevel - 1) : (bp.ProdLevel / (decimal)(1 + bp.ProdLevel));
            return bp.Details.productionTime * (1 - ((decimal)bp.Details.productivityModifier / (decimal)bp.Details.productionTime) * (PEFactor)) * productionTimeModifier;
        }

        public static string FormatSeconds(this decimal _seconds) 
        {
            int seconds = (int)(_seconds);
            int hours = (int)(seconds / 3600);
            seconds -= (hours * 3600);
            int minutes = (int)(seconds / 60);
            seconds -= (minutes * 60);
            string result = "";
            if (hours > 0) result += hours + "h ";
            if (minutes > 0) result += minutes + "m ";
            if (seconds > 0) result += seconds + "s";
            if (!String.IsNullOrWhiteSpace(result)) return result.Trim();
            return "0s";
        }

        public static string Format(this ISK isk)
        {
            return isk.ToDecimal().FormatISK();
        }

        public static string FormatISK(this decimal isk)
        {
            return isk.ToString("N2");
        }

        public static void AddToCount<T>(this Dictionary<T, int> dic, T key, int count = 1)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, 0);
            }
            dic[key] += count;
        }
	}

	public class BlueprintTests
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

<<<<<<< HEAD
=======
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

>>>>>>> 525a2ba2ec4cc2beae08a2fba3fc2e0a3c50f35e
        [TestCase("Archon")][TestCase("Aeon")]
        [TestCase("Chimera")][TestCase("Wyvern")]
        [TestCase("Thanatos")][TestCase("Nyx")]
        [TestCase("Nidhoggur")][TestCase("Hel")]
        public void TestMatPercentages(string itemName)
        {
            Console.WriteLine(CommonQueries.GetTypeID("Capital Armor Plates")  +"/"+ CommonQueries.GetBlueprintFromProduct(CommonQueries.GetMaterialID("Capital Armor Plates")));
            Console.WriteLine(itemName + ": "+ CommonQueries.GetTypeID(itemName));
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
                prices.Add(kvp.Key, provider.GetPrice(kvp.Key).ToDecimal()*kvp.Value);
            }

            var total = prices.Aggregate(0m, (current, kvp) => current.ToDecimal() + kvp.Value.ToDecimal());

            foreach (var kvp in prices)
            {
                Console.WriteLine(CommonQueries.GetTypeName(kvp.Key) + ": " + 100*(kvp.Value.ToDecimal()/total));
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

        [TestCase(@" Drake 1 Battlecruiser")]
        public void TestContractValue(string contractPaste)
        {
            var mats = new Dictionary<MaterialID, int>();
            var reg = new Regex(@"([a-zA-Z\d'\- ]*)\t\d+\.*");
            foreach (var line in contractPaste.Split(new[]{'\n'})) 
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
                Console.WriteLine(CommonQueries.GetTypeName(kvp.Key) + " x" + kvp.Value + " : "+ prices[kvp.Key].Format());
            }

            var total = prices.Aggregate(0m, (current, kvp) => current.ToDecimal() + kvp.Value.ToDecimal());

            Console.WriteLine("total: "+ total.FormatISK());
        }

		[Test]
		public void CapComponentsProfit()
		{
			var priceProvider = new BasicPriceCache(new BasicEveCentralJitaPriceProvider());
			var bpTypes = CommonQueries.GetTypesWithNamesLike("Capital % Blueprint").Select(tID => new BlueprintID(tID.ToInt()));
			foreach (var bpType in bpTypes)
			{
				var bp = new T1Blueprint(bpType, 5, 5);
				Console.WriteLine(CommonQueries.GetTypeName(bp.Product)+" Mats: ");
				var totalPrice = 0m;
				foreach (var mat in bp.Materials)
				{
					var price = priceProvider.GetPrice(mat.matID).ToDecimal()*mat.quantity*mat.damagePerJob;
					totalPrice += price;
					Console.WriteLine(CommonQueries.GetTypeName(mat.matID) + ": "+ price.FormatISK());
				}
				Console.WriteLine();
				Console.WriteLine("Total price: "+ totalPrice.FormatISK());
				var productPrice = priceProvider.GetPrice(bp.Product);
				Console.WriteLine("Product price: "+ productPrice);
				var profit = productPrice.ToDecimal() - totalPrice;
				Console.WriteLine("Profit: "+profit.FormatISK());
			}
		}	
	}
}
