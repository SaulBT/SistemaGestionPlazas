using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

public class BotonViewComponent : ViewComponent
{

    
    public IViewComponentResult Invoke(string texto = "", string tipo = "", string accion = "", bool disabled = false, bool fondo = true, string buttonType = "button", string onClick = "", string id ="", BotonModel botonModel = null)
    {
        if (botonModel != null)
        {
            ConfigurarTipoAccion(botonModel);
            return View(botonModel);
        }

        var model = new BotonModel
        {
            Texto = texto,
            Tipo = tipo.ToLower(),
            Accion = accion.ToLower(),
            Disabled = disabled,
            Icono = "",
            Fondo = fondo,
            ButtonType = buttonType,
            OnClick = onClick,
            Id = id
        };
        ConfigurarTipoAccion(model);

        return View(model);
    }

    // Configurar botones con texto predefinido
    private void ConfigurarTipoAccion(BotonModel model)
    {
        if (model.Fondo)
        {
            string tipo = model.Tipo;
            string texto = model.Texto;
            switch (model.Accion.ToLower())
            {
                //Botones con colores específicos según la acción
                case "cancelar":
                    model.Tipo = model.Accion;
                    model.Texto = "Cancelar";
                    model.Icono = "bi bi-x-lg";
                    break;
                case "publicar":
                    model.Tipo = model.Accion;
                    model.Texto = "Publicar";
                    break;
                case "ver mas":
                    model.Icono = "bi bi-three-dots";
                    model.Tipo = "terciario";
                    break;
                //Botones terciarios
                case "imprimir":
                    model.Tipo = "terciario";
                    model.Texto = "Imprimir";
                    model.Icono = "bi bi-printer";
                    break;
                //Botones secundarios 
                case "siguiente":
                    model.Tipo = "secundario";
                    model.Texto = "Siguiente";
                    model.Icono = "bi bi-arrow-right";
                    break;
                case "regresar":
                    model.Tipo = "secundario";
                    model.Texto = "Regresar";
                    model.Icono = "bi bi-arrow-left";
                    break;
                case "cargar":
                    model.Tipo = "secundario";
                    model.Texto = "Cargar";
                    model.Icono = "bi bi-upload";
                    break;
                case "buscar":
                    model.Tipo = "secundario";
                    model.Texto = "Buscar";
                    model.Icono = "bi bi-search";
                    model.ButtonType = "submit";
                    break;
                case "firmar":
                    model.Tipo = "secundario";
                    model.Texto = "Firmar";
                    model.Icono = "bi bi-feather";
                    break;
                case "ver":
                    model.Icono = "bi bi-eye-fill";
                    model.Tipo = "secundario";
                    break;
                //Botones primarios 
                case "guardar":
                    model.Tipo = "primario";
                    model.Texto = "Guardar";
                    model.Icono = "bi bi-floppy-fill";
                    break;
                case "agregar":
                    model.Tipo = "primario";
                    model.Texto = "Agregar";
                    model.Icono = "bi bi-plus-lg";
                    break;
                case "descargar":
                    model.Tipo = "primario";
                    model.Texto = "Descargar";
                    model.Icono = "bi bi-download";
                    break;
                case "enviar":
                    model.Tipo = "primario";
                    model.Texto = "Enviar";
                    model.Icono = "bi bi-send";
                    break;
                case "confirmar":
                    model.Tipo = "primario";
                    model.Texto = "Confirmar";
                    model.Icono = "bi bi-check-circle-fill";
                    break;
                default:
                    break;
            }
            if (!tipo.IsNullOrEmpty())
            {
                model.Tipo = tipo;
            }
            if (!texto.IsNullOrEmpty())
            {
                model.Texto = texto;
            }
        } else
        {
            switch (model.Accion.ToLower())
            {
                // Botones sin fondo, únicamente ícono
                case "editar":
                    model.Icono = "bi bi-pencil-fill";
                    break;
                case "eliminar":
                    model.Icono = "bi bi-trash-fill";
                    break;
                case "ver":
                    model.Icono = "bi bi-eye-fill";
                    break;
                case "archivar":
                    model.Icono = "bi bi-folder-fill";
                    break;
                case "informacion":
                case "info":
                    model.Icono = "bi bi-info-circle-fill";
                    break;
                case "programa educativo":
                case "programa":
                    model.Icono = "bi bi-mortarboard-fill";
                    break;
                case "plan de estudios":
                case "plan":
                    model.Icono = "bi bi-journal-text";
                    break;
                case "agregar":
                    model.Icono = "bi bi-plus-circle";
                    break;
                case "aspirante":
                    model.Icono = "bi bi-person-fill";
                    break;
                case "derecha":
                    model.Icono = "bi bi-arrow-right-circle-fill";
                    break;
                case "izquierda":
                    model.Icono = "bi bi-arrow-left-circle-fill";
                    break;
                case "firmar":
                    model.Icono = "bi bi-feather";
                    break;
                case "solicitudes":
                    model.Icono = "bi bi-journals";
                    break;
                case "historial":
                    model.Icono = "bi bi-clock-history";
                    break;
                case "excel":
                    model.Icono = "bi bi-file-earmark-spreadsheet-fill";
                    break;
                case "pdf":
                    model.Icono = "bi bi-file-earmark-pdf-fill";
                    break;
                default:
                    break;
            }
        }
        

    }


    
}