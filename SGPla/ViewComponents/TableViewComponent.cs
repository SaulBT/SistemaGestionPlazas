using Microsoft.AspNetCore.Mvc;

namespace SGPla.ViewComponents
{
    public class TableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TableModel model)
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