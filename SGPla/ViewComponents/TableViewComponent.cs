using Microsoft.AspNetCore.Mvc;

namespace SGPla.ViewComponents
{
    public class TableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TableModel model)
        {
            return View(model);
        }
    }
}
