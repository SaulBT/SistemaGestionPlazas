using System.Data;
using SGPla.Models.DTOs.Integracion;

namespace SGPla.Services.Implementations;

internal sealed class HorarioSesionDataReader(IReadOnlyList<PlaneaSesionValidada> sesiones, int sincronizacionId) : IDataReader
{
    internal static readonly string[] Columnas =
    ["programacion_academica_id", "sincronizacion_planea_id", "dia_semana", "hora_inicio", "hora_fin",
     "fecha_inicio", "fecha_fin", "edificio", "aula"];
    private static readonly Type[] Tipos =
    [typeof(int), typeof(int), typeof(byte), typeof(TimeSpan), typeof(TimeSpan), typeof(DateTime), typeof(DateTime), typeof(string), typeof(string)];
    private int _indice = -1;
    private PlaneaSesionValidada Actual => sesiones[_indice];

    public int FieldCount => Columnas.Length;
    public bool Read() => ++_indice < sesiones.Count;
    public string GetName(int i) => Columnas[i];
    public int GetOrdinal(string name) => Array.FindIndex(Columnas, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
    public Type GetFieldType(int i) => Tipos[i];
    public string GetDataTypeName(int i) => Tipos[i].Name;
    public object GetValue(int i) => i switch
    {
        0 => Actual.ProgramacionAcademicaId,
        1 => sincronizacionId,
        2 => Actual.DiaSemana,
        3 => Actual.HoraInicio.ToTimeSpan(),
        4 => Actual.HoraFin.ToTimeSpan(),
        5 => Actual.FechaInicio.ToDateTime(TimeOnly.MinValue),
        6 => Actual.FechaFin.ToDateTime(TimeOnly.MinValue),
        7 => (object?)Actual.Edificio ?? DBNull.Value,
        8 => (object?)Actual.Aula ?? DBNull.Value,
        _ => throw new IndexOutOfRangeException()
    };
    public int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < count; i++) values[i] = GetValue(i);
        return count;
    }
    public bool IsDBNull(int i) => GetValue(i) is DBNull;
    public object this[int i] => GetValue(i);
    public object this[string name] => GetValue(GetOrdinal(name));
    public int Depth => 0;
    public bool IsClosed => false;
    public int RecordsAffected => -1;
    public void Close() { }
    public bool NextResult() => false;
    public DataTable GetSchemaTable()
    {
        var table = new DataTable();
        table.Columns.Add("ColumnName", typeof(string));
        table.Columns.Add("ColumnOrdinal", typeof(int));
        table.Columns.Add("DataType", typeof(Type));
        for (var i = 0; i < FieldCount; i++) table.Rows.Add(Columnas[i], i, Tipos[i]);
        return table;
    }
    public bool GetBoolean(int i) => Convert.ToBoolean(GetValue(i));
    public byte GetByte(int i) => Convert.ToByte(GetValue(i));
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
    public char GetChar(int i) => Convert.ToChar(GetValue(i));
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
    public IDataReader GetData(int i) => throw new NotSupportedException();
    public DateTime GetDateTime(int i) => Convert.ToDateTime(GetValue(i));
    public decimal GetDecimal(int i) => Convert.ToDecimal(GetValue(i));
    public double GetDouble(int i) => Convert.ToDouble(GetValue(i));
    public float GetFloat(int i) => Convert.ToSingle(GetValue(i));
    public Guid GetGuid(int i) => (Guid)GetValue(i);
    public short GetInt16(int i) => Convert.ToInt16(GetValue(i));
    public int GetInt32(int i) => Convert.ToInt32(GetValue(i));
    public long GetInt64(int i) => Convert.ToInt64(GetValue(i));
    public string GetString(int i) => Convert.ToString(GetValue(i))!;
    public void Dispose() { }
}
