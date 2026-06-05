public class TableExperienciasModel
{
    public string TableId { get; set; } = "tabla";
    public List<string> Headers { get; set; } = new();
    public List<TableExperienciasRowModel> Rows { get; set; } = new();

    // Propiedades de paginación
    public PaginationInfo Pagination { get; set; } = new();
}

public class TableExperienciasRowModel
{
    public string RowId { get; set; } = string.Empty;
    public bool EsNueva { get; set; } = false;
    public bool EsEditada { get; set; } = false;
    public bool EsInvalida { get; set; } = false;
    public List<TableCellModel> Cells { get; set; } = new();
}