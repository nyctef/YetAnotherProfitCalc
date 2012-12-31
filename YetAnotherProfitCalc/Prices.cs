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
     * change interface so it can take multiple typeIds at once and do a bundle request
     * 
     */

	internal interface IPriceProvider
	{
		ISK GetPrice(TypeID typeId);
		Task<ISK> GetPriceAsync(TypeID typeId);
	}

	internal class BasicEveCentralJitaPriceProvider : IPriceProvider
	{
		public ISK GetPrice(TypeID typeId)
		{
			var result = FetchEveCentralPrice.FetchPrice(typeId.ToInt(), 10000002, 30000142);
			//Console.WriteLine("Price of " + CommonQueries.GetTypeName(typeId) + " is " + result);
			return new ISK(result);
		}

		public async Task<ISK> GetPriceAsync(TypeID typeId)
		{
			var result = await FetchEveCentralPrice.FetchPriceAsync(typeId.ToInt(), 10000002, 30000142);
			return new ISK(result);
		}
	}

    internal class BasicEveCentralDelvePriceProvider : IPriceProvider
    {
        public ISK GetPrice(TypeID typeId)
        {
            var result = FetchEveCentralPrice.FetchPrice(typeId.ToInt(), 10000060);
            return new ISK(result);
        }

        public async Task<ISK> GetPriceAsync(TypeID typeId)
        {
            var result = await FetchEveCentralPrice.FetchPriceAsync(typeId.ToInt(), 10000060);
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

        public ISK GetPrice(TypeID typeId)
        {
            if (cache.ContainsKey(typeId)) return cache[typeId];

            var result = baseProvider.GetPrice(typeId);
            cache[typeId] = result;
            return result;
        }
        
        public async Task<ISK> GetPriceAsync(TypeID typeId)
		{
            if (cache.ContainsKey(typeId)) return cache[typeId];

            var result = await baseProvider.GetPriceAsync(typeId);
            cache[typeId] = result;
            return result;
		}
    }
}
