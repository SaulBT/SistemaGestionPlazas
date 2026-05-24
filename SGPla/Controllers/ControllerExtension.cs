namespace SGPla.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SGPla.Commons;

    public static class ControllerExtension
    {
        public static void LanzarError<T>(this Controller controller, ILogger<T> logger, Exception? ex, string ubicacion, string metodo, string log, string toast)
        {
            if (ex != null)
                logger.LogError(ex, Constantes.LOGS_ESTRUCTURA, ubicacion, metodo, log);
            else
                logger.LogError(Constantes.LOGS_ESTRUCTURA, ubicacion, metodo, log);
            controller.TempData["Error"] = toast;
        }

        public static void LanzarError<T>(this Controller controller, ILogger<T> logger, Exception? ex, string ubicacion, string metodo, string log)
        {
            if (ex != null)
                logger.LogError(ex, Constantes.LOGS_ESTRUCTURA, ubicacion, metodo, log);
            else
                logger.LogError(Constantes.LOGS_ESTRUCTURA, ubicacion, metodo, log);
            controller.TempData["Error"] = Constantes.TOAST_ERROR_GENERAL;
        }
    }
}
