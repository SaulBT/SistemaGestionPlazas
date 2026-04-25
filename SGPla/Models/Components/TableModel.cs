public class TableModel
{
    public List<string> Headers { get; set; } = new();
    public List<TableRowModel> Rows { get; set; } = new();
}

public class TableRowModel
{
    public List<TableCellModel> Cells { get; set; } = new();
}

public class TableCellModel
{
    public string Value { get; set; } = string.Empty;

    // Para acciones
    public List<TableActionModel>? Actions { get; set; }
}

public class TableActionModel
{
    public string Accion { get; set; } = ""; // editar, eliminar, etc.
    public string Url { get; set; } = "";
}