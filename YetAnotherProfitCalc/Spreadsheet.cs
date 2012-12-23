using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YetAnotherProfitCalc
{
    public interface Spreadsheet
    {
        string Export();
        string GetCellLocation(Cell cell);
        Cell AddCell(Cell cell, int xpos, int ypos);
        int Height { get; }
        int Width { get; }
    }

    public abstract class Cell
    {
        public abstract string CellText(Spreadsheet spreadsheet);
        public string CellLocation(Spreadsheet spreadsheet) { return spreadsheet.GetCellLocation(this); }
    }

    public class TSVSpreadsheet : Spreadsheet
    {
        private static char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private int xmax = 0;
        private int ymax = 0;

        public int Height { get { return ymax; } }
        public int Width { get { return xmax; } }

        public Cell AddCell(Cell cell, int xpos, int ypos) 
        {
            var pos = Tuple.Create(xpos, ypos);
            spreadsheet.Add(pos, cell);
            cellpositions.Add(cell, pos);
            if (xpos > xmax) xmax = xpos;
            if (ypos > ymax) ymax = ypos;
            return cell;
        }

        private Dictionary<Tuple<int, int>, Cell> spreadsheet   = new Dictionary<Tuple<int, int>, Cell>();
        private Dictionary<Cell, Tuple<int, int>> cellpositions = new Dictionary<Cell, Tuple<int, int>>();

        public string Export()
        {
            var str = new StringBuilder();
            for (int y = 0; y <= ymax; y++)
            {
                for (int x = 0; x <= xmax; x++)
                {
                    var pos = Tuple.Create(x, y);
                    if (spreadsheet.ContainsKey(pos))
                    {
                        str.Append(spreadsheet[pos].CellText(this));
                    }
                    str.Append('\t');
                }
                str.Append('\n');
            }
            return str.ToString();
        }

        public string GetCellLocation(Cell cell)
        {
            return GetCellLocation(cellpositions[cell]);            
        }

        public string GetCellLocation(Tuple<int, int> pos)
        {
            // this uses absolute cell references because google docs tries to 
            // be clever and "fix" the references when we paste in the spreadsheet
            return "$" + alpha[pos.Item1] + "$" + (pos.Item2 + 1);
        }
    }

    public class SimpleCell : Cell
    {
        private string text;
        public SimpleCell(string text)
        {
            this.text = text;
        }
        public override string CellText(Spreadsheet spreadsheet)
        {
            return text;
        }
    }

    public class FormulaCell : Cell
    {
        private IEnumerable<Cell> cellReferences;
        private string text;

        // takes input like "={0}+{1}" where 0 and 1 refer to the passed-in cells
        public FormulaCell(string text, params Cell[] references)
        {
            this.text = text;
            this.cellReferences = references;
        }

        public override string CellText(Spreadsheet spreadsheet)
        {
            var result = text;
            var refCount = 0;
            foreach (var reference in cellReferences)
            {
                result = result.Replace("{" + refCount + "}", spreadsheet.GetCellLocation(reference));
                refCount++;
            }
            return result;
        }
    }

    public class EveCentralCell : Cell
    {
        private string text;

        public EveCentralCell(TypeID typeId, int region = -1, int system = 30000142, PriceType type = PriceType.sell, PriceMeasure measure = PriceMeasure.min)
        {
            var uri = FetchEveCentralPrice.GetUriForRequest(typeId.ToInt(), region, system);
            var xpath = "/evec_api/marketstat/type/"+type.ToString()+"/"+measure.ToString();
            text = "=ImportXML(\"" + uri + "&clock=\"&GoogleClock(), \"" + xpath + "\")";
        }

        public override string CellText(Spreadsheet spreadsheet)
        {
			if (spreadsheet is TSVSpreadsheet)
			{
				return text;
			}

	        throw new NotImplementedException();
        }
    }

    
}
