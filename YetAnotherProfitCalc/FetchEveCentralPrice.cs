using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;

namespace YetAnotherProfitCalc
{
	public enum PriceType
	{
		buy, sell, all
	}

	public enum PriceMeasure
	{
		volume, avg, max, min, stddev, median, percentile
	}

	public class FetchEveCentralPrice
	{
		public const string m_BaseUri = @"http://api.eve-central.com/api/marketstat?";

		public static Uri GetUriForRequest(int typeId, int region = -1, int system = -1)
		{
			var result = m_BaseUri + "typeid=" + typeId;
			if (region >= 0) result += "&regionlimit=" + region;
			if (system >= 0) result += "&usesystem=" + system;

			return new Uri(result);
		}

		public static async Task<decimal> FetchPriceAsync(int typeId, int region = -1, int system = -1, PriceType type = PriceType.sell, PriceMeasure measure = PriceMeasure.min)
		{
			try
			{
				var webClient = new WebClient();
				var uri = GetUriForRequest(typeId, region, system);
				var xml = await webClient.DownloadStringTaskAsync(uri);
				var xdoc = XDocument.Parse(xml);
				return Decimal.Parse(xdoc.Descendants(type.ToString()).Single().Descendants(measure.ToString()).Single().Value);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				return -1;
			}
		}

		/// <summary>
		/// Blocks on FetchPriceAsync. For testing purposes only
		/// </summary>
		public static decimal FetchPrice(int typeId, int region = -1, int system = -1, PriceType type = PriceType.sell, PriceMeasure measure = PriceMeasure.min)
		{
			var fetchPriceAsync = FetchPriceAsync(typeId, region, system, type, measure);
			return fetchPriceAsync.Result;
		}
	}

	
}
