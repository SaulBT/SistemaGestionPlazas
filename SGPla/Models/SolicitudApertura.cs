namespace SGPla.Models;

public partial class SolicitudApertura
{
    public int IdSolicitudApertura { get; set; }

    public int IdExperienciaEducativa { get; set; }

    public int IdPeriodo { get; set; }

    public int IdEntidadAcademica { get; set; }

    public int IdProgramaEducativo { get; set; }

    public int IdPlanEstudios { get; set; }

    public int IdModalidad { get; set; }

    public string Seccion { get; set; } = null!;

    public int CantidadSolicitantes { get; set; }

    public string? Justificacion { get; set; }

    public int IdArchivoOficio { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Archivo IdArchivoOficioNavigation { get; set; } = null!;

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;

    public virtual Modalidad IdModalidadNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;

    public virtual ProgramaEducativo IdProgramaEducativoNavigation { get; set; } = null!;
}
