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

	public class GroupID : TypeID
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

	public class CategoryID : TypeID
	{
		public static readonly CategoryID Skill = new CategoryID(16);

		public CategoryID(int value) : base(value) { }
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
					return DC18;
				case "Classic Doctrine":
				case "Prototype Diagram":
				case "Test Reports":
				case "Advanced Theories":
					return DC12;
				case "Formation Layout":
				case "Tuning Instructions":
				case "Collision Measurements":
				case "Calibration Data":
					return DC11;
				case "Sacred Manifesto": 
				case "User Manual": 
				case "Engagement Plan": 
				case "Operation Handbook":
					return DC10;
				case "Circular Logic": 
				case "Alignment Chart": 
				case "Symbiotic Figures": 
				case "Circuitry Schematics":
					return DC06;
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
		
		public Decryptor(decimal inventionMod, int runsMod, int mlResult, int plResult)
		{
			InventionMod = inventionMod;
			RunsMod = runsMod;
			MlResult = mlResult;
			PlResult = plResult;
		}
	}



    /*
     * 
     * Ideas:
     * eft fit price checker
     * pull multiple typeIDs from e-c at once
     * display age/reliability of prices retrieved
     * 
     */



}
