using System.Security.Claims;

namespace SGPla.Helpers
{


    public static class MatrizPermisos
    {
        private static readonly Dictionary<string, HashSet<string>> _permisos = new()
        {
            ["Entidad Academica"] =
            [
            Acciones.ProgramacionAcademica.Ver,
            Acciones.ProgramacionAcademica.VerHistorial,
            Acciones.ProgramacionAcademica.Editar,
            Acciones.ProgramacionAcademica.RetirarDocente,
            Acciones.ProgramacionAcademica.AsignarDocente,
            Acciones.ProgramacionAcademica.Eliminar,
            Acciones.ProgramacionAcademica.Ofertar,
        ],
            ["Coordinador DGAA"] =
            [

            Acciones.ProgramacionAcademica.Ver,
            Acciones.ProgramacionAcademica.VerSolicitudes,
        ],
            ["Superusuario"] =
            [
                // ...
        ]
        };

        public static AccionesDisponibles Para(ClaimsPrincipal usuario)
        {
            var acciones = usuario.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .SelectMany(c => _permisos.TryGetValue(c.Value, out var permisos)
                    ? permisos
                    : [])
                .ToHashSet();

            return new AccionesDisponibles(acciones);
        }
    }
}
