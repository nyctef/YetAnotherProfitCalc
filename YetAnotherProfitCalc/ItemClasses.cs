using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NUnit.Framework;
//using JetBrains.Annotations;

namespace YetAnotherProfitCalc
{

	/// <summary>
	/// Wrap around a type (usually a numeric value type like long or decimal) to give it extra type safety
	/// </summary>
	/// <remarks>This is mostly used to make sure that typeIDs, blueprint IDs, groupIDs etc don't get all mixed up</remarks>
	/// <typeparam name="T">The type to wrap around</typeparam>
    public class PrimitiveWrapper<T> : IEquatable<PrimitiveWrapper<T>>, IComparable<PrimitiveWrapper<T>> where T : IComparable<T>
    {
        private readonly T value;

        public PrimitiveWrapper(T value) { this.value = value; }

        public bool Equals(PrimitiveWrapper<T> other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is T)
                return value.Equals(obj);
 	        if (obj is PrimitiveWrapper<T>) 
                return Equals((PrimitiveWrapper<T>)obj);
            else 
                return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public int CompareTo(PrimitiveWrapper<T> other)
        {
            return value.CompareTo(other.value);
        }

		public static bool operator ==(PrimitiveWrapper<T> us, PrimitiveWrapper<T> them)
		{
			return ReferenceEquals(us, them) || // handles us == them == null
				(!ReferenceEquals(us, null) && us.Equals(them));
		}

	    public static bool operator !=(PrimitiveWrapper<T> us, PrimitiveWrapper<T> them)
	    {
		    return !ReferenceEquals(us, them) && // handles us == them == null
		             (ReferenceEquals(us, null) || !us.Equals(them));
	    }
    }

	public class TypeID : PrimitiveWrapper<int>
	{
		public TypeID(int value) : base(value) { }
	}

	public class MaterialID : TypeID
    {
        public MaterialID(int value) : base(value) { }
    }

	public class BlueprintID : TypeID
    {
        public BlueprintID(int value) : base(value) { }
    }

    public class GroupID : PrimitiveWrapper<int>
    {
        #region groupIDs
        public static readonly GroupID DataInterfaces = new GroupID(716);
        public static readonly GroupID Battlecruiser = new GroupID(419);
        public static readonly GroupID Battleship = new GroupID(27);
        public static readonly GroupID Cruiser = new GroupID(26);
        public static readonly GroupID Industrial = new GroupID(28);
        public static readonly GroupID Frigate = new GroupID(25);
        public static readonly GroupID Destroyer = new GroupID(420);
        public static readonly GroupID Freighter = new GroupID(513);

        #endregion

        public GroupID(int value) : base(value) { }
	}

    public class CategoryID : PrimitiveWrapper<int>
	{
		public static readonly CategoryID Skill = new CategoryID(16);

		public CategoryID(int value) : base(value) { }
	}

    public class AttributeID : PrimitiveWrapper<int>
    {
        public AttributeID(int value) : base(value) { }
    }

    public class AttributeCategory
    {
        #region Categories

        // TODO: actually we should probably populate this from dgmAttributeCategories. Oh well.

        public static AttributeCategory Get(int id)
        {
            switch (id) {
                case 1: return Fitting;
                case 2: return Shield;
                case 3: return Armor;
                case 4: return Structure;
                case 5: return Capacitor;
                case 6: return Targeting;
                case 7: return Miscellaneous;
                case 8: return Required;
                case 9: return NULL;
                case 10: return Drones;
                case 12: return AI;
                default: throw new ArgumentOutOfRangeException("id");
            }
        }

        public static AttributeCategory Fitting = new AttributeCategory(1,"Fitting","Fitting capabilities of a ship");
        public static AttributeCategory Shield = new AttributeCategory(2, "Shield", "Shield attributes of ships");
        public static AttributeCategory Armor = new AttributeCategory(3, "Armor", "Armor attributes of ships");
        public static AttributeCategory Structure = new AttributeCategory(4, "Structure", "Structure attributes of ships");
        public static AttributeCategory Capacitor = new AttributeCategory(5, "Capacitor", "Capacitor attributes for ships");
        public static AttributeCategory Targeting = new AttributeCategory(6, "Targeting", "Targeting Attributes for ships");
        public static AttributeCategory Miscellaneous = new AttributeCategory(7, "Miscellaneous", "Misc. attributes");
        public static AttributeCategory Required = new AttributeCategory(8, "Required Skills", "Skill requirements");
        public static AttributeCategory NULL = new AttributeCategory(9, "NULL", "Attributes already checked and not going into a category");
        public static AttributeCategory Drones = new AttributeCategory(10, "Drones", "All you need to know about drones");
        public static AttributeCategory AI = new AttributeCategory(12, "AI", "Attribs for the AI configuration");

        #endregion

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ID { get; private set; }

        public AttributeCategory(int id, string name, string desc) 
        {
            ID = id;
            Name = name;
            Description = desc;
        }
    }

    public class Attribute
    {
        public AttributeID ID { get; private set; }
        public string AttributeName { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public UnitID UnitID { get; private set; }

        public Attribute(AttributeID id, string name, string dName, string desc, UnitID unitID)
        {
            ID = id;
            AttributeName = name;
            DisplayName = dName;
            Description = desc;
            UnitID = unitID;
        }
    }

    public class AttributeValue<T> // is int or float
    {
        public Attribute Attribute { get; private set; }
        public T Value { get; private set; }
        public AttributeValue(Attribute attr, T value) 
        {
            Attribute = attr;
            Value = value;
        }
    }

    public class UnitID : PrimitiveWrapper<int>
    {
        public UnitID(int value) : base(value) { }
    }

    public class Unit
    {
        public UnitID ID { get; private set; }
        public string UnitName { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        public Unit(UnitID id, string name, string dName, string desc)
        {
            ID = id;
            UnitName = name;
            DisplayName = name;
            Description = desc;
        }

        public bool IsPrefix
        {
            get
            {
                return ID == new UnitID(139) || // Bonus: +X
                    ID == new UnitID(140) || // Level X
                    ID == new UnitID(136); // Slot X
            }
        }
    }

    public class ISK : PrimitiveWrapper<decimal>
    {
        public ISK(decimal value) : base(value) { }

        public static implicit operator ISK(decimal l)
        {
            return new ISK(l);
        }
    }

	public class Decryptor
	{
		public static Decryptor Get(string name)
		{
			switch (name)
			{
				case "War Strategon":
				case "Installation Guide":
				case "Stolen Formulas":
				case "Assembly Instructions":
					return DC18.WithName(name);
				case "Classic Doctrine":
				case "Prototype Diagram":
				case "Test Reports":
				case "Advanced Theories":
                    return DC12.WithName(name);
				case "Formation Layout":
				case "Tuning Instructions":
				case "Collision Measurements":
				case "Calibration Data":
                    return DC11.WithName(name);
				case "Sacred Manifesto": 
				case "User Manual": 
				case "Engagement Plan": 
				case "Operation Handbook":
                    return DC10.WithName(name);
				case "Circular Logic": 
				case "Alignment Chart": 
				case "Symbiotic Figures": 
				case "Circuitry Schematics":
                    return DC06.WithName(name);
			}
			return null;
		}

		// TODO check
		public static readonly Decryptor DCNA = new Decryptor(1.0m, 0, -4, -4);
		public static readonly Decryptor DC06 = new Decryptor(0.6m, 9, -6, -2);
		public static readonly Decryptor DC10 = new Decryptor(1.0m, 1, -3, 0);
		public static readonly Decryptor DC11 = new Decryptor(1.1m, 0, -1, -1);
		public static readonly Decryptor DC12 = new Decryptor(1.2m, 1, -2, -1);
		public static readonly Decryptor DC18 = new Decryptor(1.8m, 3, -5, -2);

		public decimal InventionMod { get; private set; }
		public int RunsMod { get; private set; }
		public int MlResult { get; private set; }
		public int PlResult { get; private set; }
        public string Name { get; private set; }
		
		public Decryptor(decimal inventionMod, int runsMod, int mlResult, int plResult)
		{
			InventionMod = inventionMod;
			RunsMod = runsMod;
			MlResult = mlResult;
			PlResult = plResult;
		}

        public Decryptor WithName(string name)
        {
            Name = name;
            return this;
        }

        public MaterialID MaterialID {
            get { return CommonQueries.GetMaterialID(Name); }
        }
	}
}
