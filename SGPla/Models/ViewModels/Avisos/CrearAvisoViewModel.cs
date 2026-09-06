using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Horario;
using SGPla.Models.DTOs.Oferta;

namespace SGPla.Models.ViewModels.Avisos
{
    public class CrearAvisoViewModel
    {
        private const string campo_obligatorio = Constantes.CAMPO_OBLIGATORIO;


        //Datos del formulario

        public int? IdAviso { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public int? IdArticulo { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string FechaVacantes { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public int? IdPeriodo { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Modalidad { get; set; }

        
        public string? Lugar { get; set; }

        [ValidateNever]
        public List<CrearHorarioAvisoDTO> Horarios { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string FechaCT { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Requisitos { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Correo { get; set; }
        [Required(ErrorMessage = campo_obligatorio)]
        public string Folio { get; set; }

        [ValidateNever]
        public List<int> OfertasId { get; set; } = [];

        //Tablas
        [ValidateNever]
        public TableModel TablaHorario { get; set; }
        public bool ExistenOfertas { get; set; }
        [ValidateNever]
        public List<PlanEstudiosAvisoViewModel> PlanesEstudios { get; set; }

        //Combos 

        [ValidateNever]
        public List<OptionModel> Articulos { get; set; }

        [ValidateNever]
        public List<OptionModel> Periodos { get; set; }

        [ValidateNever]
        public List<OptionModel> Modalidades { get; set; }

        //Datos formularios
        [ValidateNever]
        public FormularioHorarioViewModel Formulario { get; set; }
    }

    public class FormularioHorarioViewModel
    {
        public int IdHorario { get; set; }

        [Required(ErrorMessage = "Campo obligatorio.")]
        public TimeOnly HoraInicio { get; set; }
        [Required(ErrorMessage = "Campo obligatorio.")]
        public TimeOnly HoraTermino { get; set; }
        [Required(ErrorMessage = "Campo obligatorio.")]
        public DateOnly Fecha { get; set; }
    }

    public class PlanEstudiosAvisoViewModel
    {
        public string Nombre {  get; set; }
        public TableModel Tabla {  get; set; }
        public List<DatosOfertaAvisoDTO> Ofertas { get; set; } = [];
        public List<int> OfertasSeleccionadas { get; set; } = [];
    }
        
}
