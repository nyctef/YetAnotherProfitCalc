using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherProfitCalc
{
    /*
     * Ideas:
     * add bundlingpriceprovider which groups multiple requests within x time of each other into one request
     * change interface so it can take multiple typeIDs at once and do a bundle request
     * 
     */

	internal interface IPriceProvider
	{
		ISK GetPrice(TypeID typeID);
		Task<ISK> GetPriceAsync(TypeID typeID);
	}

	internal class BasicEveCentralJitaPriceProvider : IPriceProvider
	{
		public ISK GetPrice(TypeID typeID)
		{
			var result = FetchEveCentralPrice.FetchPrice(typeID.ToInt(), 10000002, 30000142);
			//Console.WriteLine("Price of " + CommonQueries.GetTypeName(typeID) + " is " + result);
			return new ISK(result);
		}

		public async Task<ISK> GetPriceAsync(TypeID typeID)
		{
			var result = await FetchEveCentralPrice.FetchPriceAsync(typeID.ToInt(), 10000002, 30000142);
			return new ISK(result);
		}
	}

    internal class BasicEveCentralDelvePriceProvider : IPriceProvider
    {
        public ISK GetPrice(TypeID typeID)
        {
            var result = FetchEveCentralPrice.FetchPrice(typeID.ToInt(), 10000060);
            return new ISK(result);
        }

        public async Task<ISK> GetPriceAsync(TypeID typeID)
        {
            var result = await FetchEveCentralPrice.FetchPriceAsync(typeID.ToInt(), 10000060);
            return new ISK(result);
        }
    }

    internal class BasicPriceCache : IPriceProvider
    {
        private IPriceProvider baseProvider;
        private Dictionary<TypeID, ISK> cache = new Dictionary<TypeID, ISK>();

        public BasicPriceCache(IPriceProvider baseProvider)
        {
            this.baseProvider = baseProvider;
        }

        public ISK GetPrice(TypeID typeID)
        {
            if (cache.ContainsKey(typeID)) return cache[typeID];

            var result = baseProvider.GetPrice(typeID);
            cache[typeID] = result;
            return result;
        }
        
        public async Task<ISK> GetPriceAsync(TypeID typeID)
		{
            if (cache.ContainsKey(typeID)) return cache[typeID];

            var result = await baseProvider.GetPriceAsync(typeID);
            cache[typeID] = result;
            return result;
		}
    }
}
