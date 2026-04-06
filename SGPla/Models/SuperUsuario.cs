using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class SuperUsuario
{
    public int idSuperUsuario { get; set; }

    public string nombre { get; set; } = null!;

    public string correo { get; set; } = null!;
}
