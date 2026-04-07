using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models;

public partial class Articulo
{
    public int? IdArticulo { get; set; }
    [Range(1, int.MaxValue, 
        ErrorMessage = "El número debe ser un número positivo")]
    public string Numero { get; set; } = null!;

    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "La descripción debe ser entre 3 y 100 caracteres")]

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
