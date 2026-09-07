using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SGPla.Models.Components
{
    public class HeaderUsuarioModel
    {
        public string TituloSistema { get; set; } = "Sistema de Gestión de Plazas Vacantes";
        public bool EstaAutenticado { get; set; }
        public string? Nombre { get; set; }
        public string? Rol { get; set; }
    }
}
