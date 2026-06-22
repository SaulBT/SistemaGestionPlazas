using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using SGPla.Models.Components;
using SGPla.Models.DTOs.ProgramacionAcademica;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class IndexViewModel
    {
        public List<OptionModel> Regiones { get; set; }

        public List<OptionModel> Entidades { get; set; }

        public List<OptionModel> Programas { get; set; }


        public int PaginaActual { get; set; } = 1;

        public int CantidadPorPagina { get; set; } = 10;

        public TableModel Table { get; set; }

        public string? Region { get; set; }

        public List<OptionModel> Periodos { get; set; }

        public int? IdEntidadAcademica { get; set; }

        public int? IdProgramaEducativo { get; set; }
        public int? IdPeriodo { get; set; }

        public List<ResumenOfertaProgramacionAcademicaDTO> ResumenesProgramacionesAcademicas { get; set; }



    }
}
