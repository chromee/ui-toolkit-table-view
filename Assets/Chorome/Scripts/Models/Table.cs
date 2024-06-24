namespace Chorome.Models
{
    public class Table
    {
        public Row[] Rows { get; set; }
        public Column[] Columns { get; set; }
    }

    public class Row
    {
        public int Index { get; set; }
        public float Height { get; set; }
    }

    public class Column
    {
        public int Index { get; set; }
        public float Width { get; set; }
    }
}
