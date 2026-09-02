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

            public static string ConstruirPeriodoMostrar(int anioEjercicio, string periodoCodigo)
            {
                return periodoCodigo switch
                {
                    "01" => $"Agosto {anioEjercicio - 1} – Enero {anioEjercicio}",
                    "51" => $"Febrero – Julio {anioEjercicio}",
                    _ => "Desconocido"
                };
            }
        }

        public static Periodo ToModel(CrearPeriodoEscolarDTO dto)
        {
            if (!PeriodoMapper.NombreACodigo.TryGetValue(dto.Periodo, out var periodoCodigo))
                throw new ArgumentException("Periodo inválido");

            return new Periodo
            {
                Codigo = dto.Anio + periodoCodigo
            };
        }

        public static Periodo ToModel(EditarPeriodoEscolarDTO dto)
        {
            if (!PeriodoMapper.NombreACodigo.TryGetValue(dto.Periodo, out var periodoCodigo))
                throw new ArgumentException("Periodo inválido");

            return new Periodo
            {
                IdPeriodo = dto.IdPeriodoEscolar,

                Codigo = dto.Anio + periodoCodigo
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

            int anioEjercicio = int.Parse(anio);

            return new DetallesPeriodoEscolarDTO
            {
                Codigo = codigo,

                Periodo = periodoNombre,

                Anio = anioEjercicio,

                IdPeriodoEscolar = model.IdPeriodo,

                PeriodoMostrar = PeriodoMapper.ConstruirPeriodoMostrar(
                    anioEjercicio,
                    periodoCodigo
                )
            };
        }
    }
}