using SGPla.Models.Components;
using SGPla.Models.DTOs.Aviso;

namespace SGPla.Models.ViewModels.Avisos
{
    public class IndexViewModel
    {
        public List<OptionModel> Periodos { get; set; } = [];
        public List<OptionModel> Entidades { get; set; } = [];
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin {  get; set; }

        public string Rol { get; set; } = string.Empty;
        public int? IdEntidadSeleccionada { get; set; }
        public int? IdPeriodoSeleccionado { get; set; }
        public string? Busqueda { get; set; }

        public TabAvisosViewModel Todos = new();
        public TabAvisosViewModel Creados = new();
        public TabAvisosViewModel EnRevision = new();
        public TabAvisosViewModel Avalados = new();
        public TabAvisosViewModel Devueltos = new();
        public TabAvisosViewModel Firmados = new();
        public TabAvisosViewModel Publicados = new();
        public TabAvisosViewModel ConActa = new();
        public TabAvisosViewModel Archivados = new();

        public ModalPublicarViewModel Modal { get; set; } = new ModalPublicarViewModel();

        // Propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
