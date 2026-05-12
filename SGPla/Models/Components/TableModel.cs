public class TableModel
{
    public List<string> Headers { get; set; } = new();
    public List<TableRowModel> Rows { get; set; } = new();

    // Propiedades de paginación
    public PaginationInfo Pagination { get; set; } = new();
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
    public string? Url { get; set; } = "";

    public Dictionary<string, string>? Data { get; set; }
    public string? OnClick { get; set; } = "";
}

// Nueva clase para la paginación
public class PaginationInfo
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; } = 0;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string? OnPageChange { get; set; } // Nombre de la función JavaScript
}