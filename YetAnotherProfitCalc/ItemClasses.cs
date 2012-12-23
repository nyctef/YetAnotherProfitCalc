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
        public Unit Unit { get; private set; }
        public AttributeCategory Category { get; private set; }

        public Attribute(AttributeID id, string name, string dName, string desc, Unit unit, AttributeCategory cat)
        {
            ID = id;
            AttributeName = name;
            DisplayName = dName;
            Description = desc;
            Unit = unit;
            Category = cat;
        }
    }

    public class AttributeValue
    {
        public Attribute Attribute { get; private set; }
        public int IntValue { get; private set; }
        public float FloatValue { get; private set; }
        public bool IsInt { get; private set; }

        public string Value { get { return Attribute.Unit.Format(IsInt ? IntValue.ToString() : FloatValue.ToString()); } }

        public AttributeValue(Attribute attr, int value) 
        {
            Attribute = attr;
            IntValue = value;
            IsInt = true;
        }

        public AttributeValue(Attribute attr, float value)
        {
            Attribute = attr;
            FloatValue = value;
            IsInt = false;
        }
    }

    public class UnitID : PrimitiveWrapper<int>
    {
        public UnitID(int value) : base(value) { }
    }

    public class Unit
    {
        public static Unit Get(UnitID id, string name, string dName, string desc)
        {
            switch (id.ToInt()) {
                case 140: // Level X
                case 136: // Slot X
                    return new PrefixUnit(id, name, dName, desc);
                case 139: // (Bonus) +X
                    return new BonusUnit(id, name, dName, desc);
                case 115: // groupID
                    return new GroupIdUnit(id, name, dName, desc);
                case 116: // typeID
                    return new TypeIdUnit(id, name, dName, desc);
                case 117: // size
                    return new SizeClassUnit(id, name, dName, desc);
                case 119: // attributeID
                    return new AttributeIdUnit(id, name, dName, desc);
                case 137: // bool
                    return new BoolUnit(id, name, dName, desc);
                case 118: // ore units
                case 138: // units
                case 141: // hardpoints
                    return new BlankUnit(id, name, dName, desc);
                default:
                    return new Unit(id, name, dName, desc);
            }
        }

        public UnitID ID { get; private set; }
        public string UnitName { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        protected Unit(UnitID id, string name, string dName, string desc)
        {
            ID = id;
            UnitName = name;
            DisplayName = dName;
            Description = desc;
        }

        public virtual string Format(object input)
        {
            return String.Format("{0}" + DisplayName, input);
        }
    }

    public class PrefixUnit : Unit
    {
        public PrefixUnit(UnitID id, string name, string dName, string desc) : base (id, name, dName, desc) { }

        public override string Format(object input)
        {
            return String.Format(DisplayName + " {0}", input);
        }
    }

    public class BonusUnit : Unit
    {
        public BonusUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var value = input.ToDecimal();
            if (value >= 0)
            {
                return String.Format(DisplayName + "{0}", input);
            }
            else
            {
                return input.ToString();
            }
        }
    }

    public class GroupIdUnit : Unit
    {
        public GroupIdUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var groupId = new GroupID(input.ToInt());
            return CommonQueries.GetGroupName(groupId);
        }
    }

    public class TypeIdUnit : Unit
    {
        public TypeIdUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var typeId = new TypeID(input.ToInt());
            return CommonQueries.GetTypeName(typeId);
        }
    }

    public class AttributeIdUnit : Unit
    {
        public AttributeIdUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var attrID = new AttributeID(input.ToInt());
            return CommonQueries.GetAttributeName(attrID);
        }
    }

    public class SizeClassUnit : Unit
    {
        public SizeClassUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var size = input.ToInt();
            switch (size) {
                case 1: return "Small";
                case 2: return "Medium";
                case 3: return "Large";
                default: throw new ArgumentOutOfRangeException("input");
            }
        }
    }

    public class BoolUnit : Unit
    {
        public BoolUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            var size = input.ToInt();
            switch (size)
            {
                case 1: return "True";
                case 0: return "False";
                default: throw new ArgumentOutOfRangeException("input");
            }
        }
    }

    public class BlankUnit : Unit 
    {
        public BlankUnit(UnitID id, string name, string dName, string desc) : base(id, name, dName, desc) { }

        public override string Format(object input)
        {
            return input.ToString();
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
        public MaterialID MaterialID { get; private set; }
		
		private Decryptor(decimal inventionMod, int runsMod, int mlResult, int plResult)
		{
			InventionMod = inventionMod;
			RunsMod = runsMod;
			MlResult = mlResult;
			PlResult = plResult;
		}

        private Decryptor WithName(string name)
        {
            Name = name;
            MaterialID = CommonQueries.GetMaterialID(name);
            return this;
        }
	}
}
