using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Horario;

namespace SGPla.Models.ViewModels.Avisos
{
    public class CrearAvisoViewModel
    {
        private const string campo_obligatorio = Constantes.CAMPO_OBLIGATORIO;


        //Datos del formulario

        [Required(ErrorMessage = campo_obligatorio)]
        public int IdArticulo { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Fecha { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public int IdPeriodo { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Modalidad { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Lugar { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public List<CrearHorarioDTO> Horarios { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string FechaCT { get; set; }

        [Required(ErrorMessage = campo_obligatorio)]
        public string Requisitos { get; set; }

        //Tablas

        public TableModel TablaHorario { get; set; }

        public List<TableModel> TablasCarreras { get; set; }

        //Combos 

        [ValidateNever]
        public List<OptionModel> Articulos { get; set; }

        [ValidateNever]
        public List<OptionModel> Periodos { get; set; }

        [ValidateNever]
        public List<OptionModel> Modalidades { get; set; }
    }
}
