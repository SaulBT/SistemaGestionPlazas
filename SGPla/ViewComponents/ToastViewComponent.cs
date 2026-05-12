using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Componentes;

namespace SGPla.ViewComponents
{
    public class ToastViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ToastModel model)
        {
            return View(model);
        }
    }
}