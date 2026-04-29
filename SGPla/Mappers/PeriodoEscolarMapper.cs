using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;

namespace SGPla.Mappers
{

    public class PeriodoEscolarMapper
    {
        public static class PeriodoMapper
        {
            public static readonly Dictionary<string, string> NombreACodigo = new()
            {
                { "Febrero-Julio", "51" },
                { "Agosto-Enero", "01" }
            };

            public static readonly Dictionary<string, string> CodigoANombre = new()
            {
                { "51", "Febrero-Julio" },
                { "01", "Agosto-Enero" }
            };

            public static int AnioParaCodigo(int anio) => anio + 1;

            public static int AnioParaPresentacion(int anioCodigo) => anioCodigo - 1;
        }

        public static Periodo ToModel(CrearPeriodoEscolarDTO dto)
        {
            if (!PeriodoMapper.NombreACodigo.TryGetValue(dto.Periodo, out var periodoCodigo))
                throw new ArgumentException("Periodo inválido");

            return new Periodo
            {
                Codigo = PeriodoMapper.AnioParaCodigo(dto.Anio) + periodoCodigo 
            };
        }


        public static Periodo ToModel(EditarPeriodoEscolarDTO dto)
        {
            if (!PeriodoMapper.NombreACodigo.TryGetValue(dto.Periodo, out var periodoCodigo))
                throw new ArgumentException("Periodo inválido");

            return new Periodo
            {
                IdPeriodo = dto.IdPeriodoEscolar,
                Codigo = PeriodoMapper.AnioParaCodigo(dto.Anio) + periodoCodigo 
            };
        }


        public static DetallesPeriodoEscolarDTO ToDTO(Periodo model)
        {
            string codigo = model.Codigo;

            if (string.IsNullOrEmpty(codigo) || codigo.Length != 6)
                throw new ArgumentException("Código inválido");

            string anio = codigo.Substring(0, 4);
            string periodoCodigo = codigo.Substring(4, 2);

            if (!PeriodoMapper.CodigoANombre.TryGetValue(periodoCodigo, out var periodoNombre))
                periodoNombre = "Desconocido";

            return new DetallesPeriodoEscolarDTO
            {
                Codigo = codigo,
                Periodo = periodoNombre,
                Anio = PeriodoMapper.AnioParaPresentacion(int.Parse(anio)),
                IdPeriodoEscolar = model.IdPeriodo
            };
        }
    }
}
