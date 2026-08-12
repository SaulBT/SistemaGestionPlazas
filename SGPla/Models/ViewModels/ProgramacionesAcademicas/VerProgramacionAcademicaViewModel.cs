using SGPla.Helpers;
using System.Security.Claims;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class VerProgramacionAcademicaViewModel
    {
        public string? Region { get; set; }
        public string? NombreEntidadAcademica { get; set; }
        public string? NombrePeriodo { get; set; }
        public string? NombrePrograma { get; set; }

        public TableModel TableAsignadas { get; set; } = new();
        public TableModel TableVacantes { get; set; } = new();

        //public TableModel TableCargas { get; set; }


        public AccionesDisponibles AccionesDisponibles { get; }

        public VerProgramacionAcademicaViewModel(ClaimsPrincipal usuario)
        {
            AccionesDisponibles = MatrizPermisos.Para(usuario);
        }

        public bool PuedeVerHistorial => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.VerHistorial);
        public bool PuedeEditar => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.Editar);
        public bool PuedeRetirarDocente => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.RetirarDocente);
        public bool PuedeAsignarDocente => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.AsignarDocente);
        public bool PuedeEliminar => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.Eliminar);
        public bool PuedeOfertar => AccionesDisponibles.Puede(Acciones.ProgramacionAcademica.Ofertar);



        public int IdEntidadAcademica { get; set; }

        public int IdProgramaEducativo { get; set; }

        public int IdPeriodo { get; set; }

    }
}
