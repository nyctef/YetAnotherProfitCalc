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
    
    public static class CommonQueries
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

       	public static MaterialID GetT2VersionOfT1(MaterialID matID, string outputName = null)
		{
            var result = GetMetaGroupVersionOfT1(matID, 2); // note metaGroup not metaLevel
            return result.FirstOrDefault(r => outputName != null ? CommonQueries.GetTypeName(r) == outputName : true);
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

        public static IEnumerable<Tuple<string, TypeID>> GetTypesWithNamesLike(string searchString, int limit = 30)
		{
			using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
			{
				cnn.Open();
				var query = @"select typeID, typeName from invTypes where typeName like """ + searchString + @""" LIMIT "+limit+";";
				var results = DefaultDatabase.RunSQLTableQuery(query, cnn);
				while (results.Read())
				{
                    yield return Tuple.Create(results["typeName"].ToString(), new TypeID(results["typeID"].ToInt()));
				}
			}
		}

        public static string GetTypeName(TypeID typeID)
        {
            var query = @"select typeName from invTypes where typeID = " + typeID + ";";
            return DefaultDatabase.RunSQLStringQuery(query);
        }

        public static string GetTypeDescription(TypeID typeID)
        {
            var query = @"select description from invTypes where typeID = " + typeID + ";";
            return DefaultDatabase.RunSQLStringQuery(query);
        }

        public static string GetGroupName(GroupID groupID)
        {
            var query = @"select groupName from invGroups where groupID = " + groupID + ";";
            return DefaultDatabase.RunSQLStringQuery(query);
        }

        public static string GetAttributeName(AttributeID attrID)
        {
            var query = @"select displayName from dgmAttributeTypes where attributeID = " + attrID + ";";
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

        public static decimal GetWasteForME(int ME, int wasteFactor = 10)
        {
            var factor = (ME >= 0) ? (1m / (ME + 1)) / 10m 
                                   : (1 - ME) / 10m;
            return factor * (wasteFactor/10.0m);
        }

        public static List<AttributeValue> GetAttributesForType(TypeID typeID)
        {
            var query = @"
SELECT ta.attributeID, valueInt, valueFloat, attributeName, at.description, at.displayName, at.categoryID, u.unitID, u.unitName, u.displayName, u.description
FROM dgmTypeAttributes AS ta
INNER JOIN dgmAttributeTypes AS at ON ta.attributeID = at.attributeID
INNER JOIN eveUnits AS u ON at.unitID = u.unitID
WHERE ta.typeID = "+typeID.ToInt()+@"
LIMIT 100
";
            using (var cnn = new SQLiteConnection(DefaultDatabase.dbConnection))
            {
                cnn.Open();

                var reader = DefaultDatabase.RunSQLTableQuery(query, cnn);
                var results = new List<AttributeValue>();
                while (reader.Read())
                {
                    // todo: referencing columns by name doesn't seem to support names like at.description. There must be a better workaround for that
                    var unitDesc = reader[10];
                    var unitDispName = reader[9];
                    var unit = Unit.Get(new UnitID((byte)reader[7]), 
                        (string)reader[8], 
                        unitDispName is DBNull ? "" : (string)unitDispName, 
                        unitDesc     is DBNull ? "" : (string)unitDesc);
                    var attrDispName = reader[5];
                    var attribute = new Attribute(new AttributeID((short)reader[0]), 
                        (string)reader[3],
                        attrDispName is DBNull ? "" : (string)attrDispName, 
                        (string)reader[4], 
                        unit, 
                        AttributeCategory.Get((byte)reader[6]));

                    AttributeValue value;
                    if (reader["valueFloat"] is DBNull)
                    {
                        value = new AttributeValue(attribute, (int)reader[1]);
                    }
                    else
                    {
                        value = new AttributeValue(attribute, (float)(double)reader[2]);
                    }
                    results.Add(value);
                }
                return results;
            }
        }

    }
}
