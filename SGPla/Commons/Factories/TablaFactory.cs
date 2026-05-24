namespace SGPla.Commons.Factories
{
    public static class TablaFactory
    {
        public static TableModel GenerarTablaConMensaje(List<string> headers, string mensaje)
        {
            List<TableCellModel> cells = new();
            cells.Add(new TableCellModel
            {
                Value = mensaje
            });
            for (int i = 0; i < headers.Count - 1; i++)
            {
                cells.Add(new TableCellModel());
            }

            return new TableModel
            {
                Headers = headers,
                Rows = new TableRowModel[]
                    {
                        new TableRowModel
                        {
                            Cells = cells
                        }
                    }.ToList(),
            };
        }
    }
}
