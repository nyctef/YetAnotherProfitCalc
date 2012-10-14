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



    /*
     * 
     * Ideas:
     * eft fit price checker
     * pull multiple typeIDs from e-c at once
     * display age/reliability of prices retrieved
     * 
     */



}
