using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class BuscadorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BuscadorModel model)
        {
            return View(model);
        }
    }
}