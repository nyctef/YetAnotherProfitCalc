using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using NUnit.Framework;
using System.Data.SQLite;
using YetAnotherProfitCalc;
using YetAnotherProfitCalc.Enumerations;

namespace YetAnotherProfitCalc
{
    public static class ObjectExtensions
    {
        public static int ToInt(this object obj)
        {
            return int.Parse(obj.ToString());
        }

        public static long ToLong(this object obj)
        {
            return long.Parse(obj.ToString());
        }

        public static decimal ToDecimal(this object obj)
        {
            return decimal.Parse(obj.ToString());
        }
    }
    
    static class CommonQueries
    {
        public static SQLiteDumpWrapper DefaultDatabase = new SQLiteDumpWrapper();

        public static IEnumerable<BPMaterial> GetMaterialsRaw(MaterialID material)
        {
            using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
            {
                cnn.Open();
                var query = @"SELECT m.materialTypeID, m.quantity
FROM invTypeMaterials AS m
 INNER JOIN invTypes AS t
  ON m.typeID = t.typeID
WHERE m.typeID = " + material + ";";
                var reader = DefaultDatabase.RunSQLTableQuery(query, cnn);
                var results = new List<BPMaterial>();
                while (reader.Read())
                {
					results.Add(new BPMaterial(new MaterialID(reader["materialTypeID"].ToInt()), reader["quantity"].ToLong()));
                }
                return results;
            }
        }

        public static IEnumerable<BPMaterial> GetMaterialsExtra(BlueprintID bpID, ActivityIDs activity = ActivityIDs.Manufacturing)
        {
            using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
            {
                cnn.Open();
                var query = string.Format(@"
SELECT t.typeName, r.requiredTypeID, r.quantity, r.damagePerJob
FROM ramTypeRequirements AS r
INNER JOIN invTypes AS t ON r.requiredTypeID = t.typeID
INNER JOIN invGroups AS g ON t.groupID = g.groupID
WHERE r.typeID = {0}
 AND r.activityID = {1}
 AND g.categoryID != {2};", bpID, (int)activity, CategoryID.Skill);
                var reader = DefaultDatabase.RunSQLTableQuery(query, cnn);
                var results = new List<BPMaterial>();
                while (reader.Read())
                {
                    results.Add(new BPMaterial(new MaterialID(reader["requiredTypeID"].ToInt()), reader["quantity"].ToLong(), reader["damagePerJob"].ToDecimal()));
                }
                return results;
            }

        }

        public static IEnumerable<Tuple<string, TypeID>> PossibleCompletions(string input)
        {
            using (var cnn = new SQLiteConnection(CommonQueries.DefaultDatabase.dbConnection))
            {
                cnn.Open();
                var query = @"select typeID, typeName from invMetaTypes where typeName like "" " + input + @"%"" ";
                var results = CommonQueries.DefaultDatabase.RunSQLTableQuery(query, cnn);
                while (results.Read())
                {
                    yield return Tuple.Create(results["typeName"].ToString(), new TypeID(results["typeID"].ToInt()));
                }
            }
        }

        public static IEnumerable<BPMaterial> GetMaterialsReprocessing(MaterialID material)
        {
            return GetMaterialsRaw(material); // seems like

//            var query = @"SELECT t.MaterialID, m.quantity
//FROM invTypeMaterials AS m
// INNER JOIN invTypes AS t
//  ON m.materialTypeID = t.MaterialID
//WHERE m.MaterialID =" + material + ";";
//            var results = DefaultDatabase.RunSQLTableQuery(query);

//            return from DataRow row in results.Rows
//                   select new BPMaterial(row["MaterialID"].ToInt(), row["quantity"].ToLong());
        }

        // todo: this might get called a fuckton and need caching (also GetGroupID)
        public static GroupID GetGroupID(this MaterialID matID)
        {
            var query = @"select groupID from invTypes where typeID = " + matID + ";";
            return new GroupID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

        public static CategoryID GetCategoryID(this GroupID groupID)
        {
            var query = @"select categoryID from invTypes where groupID = " + groupID + ";";
            return new CategoryID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

		public static CategoryID GetCategoryID(this MaterialID matID)
		{
			return GetCategoryID(GetGroupID(matID));
		}

        public static MaterialID GetT1VersionOfT2(MaterialID matID)
        {
            var query = @"select parentTypeID from invMetaTypes where typeID = " + matID + ";";
            return new MaterialID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

        /// <summary>
        /// Note: this assumes there exists only one t2 version, which is untrue for some items 
        /// eg fast frigates (combat/fleet inties) and prototype cloaks (improved/covops)
        /// </summary>
		public static MaterialID GetT2VersionOfT1(MaterialID matID)
		{
            var result = GetMetaGroupVersionOfT1(matID, 2); // note metaGroup not metaLevel
            if (result.Any()) return result.First();
            return new MaterialID(0);
		}

		public static IEnumerable<MaterialID> GetMetaGroupVersionOfT1(MaterialID matID, int metaGroupID)
		{
            using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
            {
                cnn.Open();
                var query = @"select typeID from invMetaTypes where parentTypeID = " + matID + " and metaGroupID = " + metaGroupID;
                var results = DefaultDatabase.RunSQLTableQuery(query, cnn);
                var t1Name = CommonQueries.GetTypeName(matID);
                while (results.Read())
                {
                    yield return new MaterialID(results["typeID"].ToInt());
                }
            }
		}

        public static string GetTypeName(TypeID matID)
        {
            var query = @"select typeName from invTypes where typeID = " + matID + ";";
            return DefaultDatabase.RunSQLStringQuery(query);
        }

        public static TypeID GetTypeID(string typeName)
        {
            var query = @"select typeID from invTypes where typeName = """ + typeName + @""" COLLATE NOCASE;";
            return new TypeID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

        public static MaterialID GetMaterialID(string typeName)
        {
            return new MaterialID(GetTypeID(typeName).ToInt());
        }

        public static BlueprintID GetBlueprintID(string typeName)
        {
            var query = @"select typeID from invTypes where typeName = """ + typeName + @" Blueprint"" COLLATE NOCASE;";
            return new BlueprintID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

        public static MaterialID GetProductFromBlueprint(BlueprintID bpID)
        {
            var query = @"select productTypeID from invBlueprintTypes where blueprintTypeID = " + bpID + ";";
            return new MaterialID(DefaultDatabase.RunSQLStringQuery(query).ToInt());
        }

        public static BlueprintID GetBlueprintFromProduct(MaterialID matID)
        {
            // not sure if there's a better way of doing this
            return GetBlueprintID(GetTypeName(matID));
        }

        public static BPDetails GetBlueprintDetails(BlueprintID bpID)
        {
            var query = @"select * from invBlueprintTypes where blueprintTypeID = " + bpID + ";";

            using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
            {
                cnn.Open();
                var reader = DefaultDatabase.RunSQLTableQuery(query, cnn);
                var results = new List<BPMaterial>();
                reader.Read();
                return new BPDetails(
                    reader["productionTime"].ToInt(),
                    reader["researchProductivityTime"].ToInt(),
                    reader["researchMaterialTime"].ToInt(),
                    reader["researchCopyTime"].ToInt(),
                    reader["researchTechTime"].ToInt(),
                    reader["productivityModifier"].ToInt(),
                    reader["materialModifier"].ToInt(),
                    reader["wasteFactor"].ToInt(),
                    reader["maxProductionLimit"].ToInt());
            }
        }

        [TestCase(0, 10, Result=0.1)]
        [TestCase(-1, 10, Result = 0.2)]
        [TestCase(-4, 10, Result = 0.5)]
        [TestCase(1, 10, Result = 0.05)]
        [TestCase(2, 10, Result = 0.1 / 3)]
        [TestCase(0, 5, Result = 0.05)]
        public static decimal GetWasteForME(int ME, int wasteFactor = 10)
        {
            var factor = (ME >= 0) ? (1m / (ME + 1)) / 10m 
                                   : (1 - ME) / 10m;
            return factor * (wasteFactor/10.0m);
        }

    }
}
