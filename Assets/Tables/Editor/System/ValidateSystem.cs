using Tables.Editor.VisualElements;
using Tables.Runtime;

namespace Tables.Editor.System
{
    public class ValidateSystem
    {
        private readonly Database _database;
        private readonly Table _table;

        public ValidateSystem(Database database, Table table)
        {
            _database = database;
            _table = table;
        }

        public void StartValidate()
        {
            foreach (var row in _table.DataRows) ValidateRow(row);
            _table.OnRowAdded += ValidateRow;
        }

        private void ValidateRow(DataRow row)
        {
            foreach (var cell in row.Cells)
            {
                cell.ChangeStatus(_database.Validate(cell.Metadata, row.Data, cell.GetValue()));
                cell.OnValueChanged += (_, current) => cell.ChangeStatus(_database.Validate(cell.Metadata, row.Data, current));
            }
        }
    }
}
