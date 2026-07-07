using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class AvisoTarjetaViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(AvisoTarjetaModel model)
        {
            return View(model);
        }
    }
}
