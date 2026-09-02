using Microsoft.AspNetCore.Mvc;

namespace SGPla.ViewComponents
{
    public class TableExperienciasViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TableExperienciasModel model)
        {
            // Validar que la paginación esté configurada
            if (model.Pagination == null)
            {
                model.Pagination = new PaginationInfo();
            }

            return View(model);
        }
    }
}