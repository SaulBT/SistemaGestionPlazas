using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class SuperUsuario
{
    public int IdSuperUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;
}
