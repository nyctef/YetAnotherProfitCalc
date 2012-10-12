using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YetAnotherProfitCalc.Enumerations;
using NUnit.Framework;

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
		// out int productionTime, out int researchProductivityTime, out int researchMaterialTime, out int researchCopyTime, out int researchTechTime, out int productivityModifier, out int materialModifier, out int wasteFactor, out int maxProductionLimit
		public readonly int productionTime;
		public readonly int researchProductivityType;
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
			this.researchProductivityType = researchProductivityType;
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
			Materials = CommonQueries.GetMaterialsRaw(Product).Union(CommonQueries.GetMaterialsExtra(bpID)).ToList();
			Details = CommonQueries.GetBlueprintDetails(bpID);

			//var extraMats = CommonQueries.GetMaterialsExtra(bpID, ActivityID.Invention).ToList();
            //InventionMaterials = extraMats
            //    .Where(m=>m.matID.GetGroupID() != GroupID.DataInterfaces) // data dump is terrible and gives data interfaces a damagePerJob of 100%
            //    .ToList();
            //InventionInterface = extraMats
            //    .Single(m => m.matID.GetGroupID() == GroupID.DataInterfaces)
            //    .matID;

			// TODO: ME waste
			// TODO: prod times
			// TODO: skills
		}
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
			Console.WriteLine(new T1Blueprint(t1BPID, 0, 0).InventionMaterials);

			Materials = new List<BPMaterial>();

			var meWastageFactor = 1 + CommonQueries.GetWasteForME(matLevel, Details.wasteFactor);

			var rawMaterials = CommonQueries.GetMaterialsRaw(Product);
			var extraMaterials = CommonQueries.GetMaterialsExtra(bpID);
			var t1Materials = CommonQueries.GetMaterialsRaw(t1ID);
			var extraT1Materials = CommonQueries.GetMaterialsExtra(t1BPID);
			// TODO: currently this assumes there is always exactly 1 recyclable material which is the t1 item
			var rawMaterialsAfterRecycling = rawMaterials.Subtract(t1Materials);
			// adjust for ME
			var adjustedRawMaterials = rawMaterialsAfterRecycling.Select(m => new BPMaterial(m.matID, (long)(m.quantity * meWastageFactor)));

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
				var newQuantity = baseType.quantity - toSubDic[baseType.matID].quantity;
				Debug.Assert(newQuantity >= 0);
				if (newQuantity > 0)
				{
					result.Add(new BPMaterial(baseType.matID, newQuantity));
				}
			}

			return result;
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
                    totals[capMatMat.matID] += capMatMat.quantity;
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

	}
}
