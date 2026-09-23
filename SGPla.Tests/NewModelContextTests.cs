using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SGPla.Data.NewModel;

namespace SGPla.Tests;

public sealed class NewModelContextTests
{
    [Fact]
    public void Model_configures_normalized_composite_keys()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer("Server=(local);Database=sgpla_model_validation;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new SgplaDbContext(options);

        Assert.Equal(2, context.Model.FindEntityType(typeof(SGPla.Data.NewModel.Entities.SolicitudDocumento))!.FindPrimaryKey()!.Properties.Count);
        Assert.Equal(2, context.Model.FindEntityType(typeof(SGPla.Data.NewModel.Entities.VotacionSolicitud))!.FindPrimaryKey()!.Properties.Count);
        Assert.Equal("academico", context.Model.FindEntityType(typeof(SGPla.Data.NewModel.Entities.AreaAcademica))!.GetSchema());

        var entidad = context.Model.FindEntityType(typeof(SGPla.Data.NewModel.Entities.EntidadAcademica))!;
        Assert.Equal("fecha_eliminacion", entidad.FindProperty(nameof(SGPla.Data.NewModel.Entities.EntidadAcademica.FechaEliminacion))!
            .GetColumnName(StoreObjectIdentifier.Table("entidad_academica", "academico")));
        Assert.Equal("campus_id", entidad.FindProperty(nameof(SGPla.Data.NewModel.Entities.EntidadAcademica.CampusId))!
            .GetColumnName(StoreObjectIdentifier.Table("entidad_academica", "academico")));
    }
}
