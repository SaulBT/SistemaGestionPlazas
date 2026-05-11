using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class ModalFormularioViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ModalFormularioModel model)
        {
            return View(model);
        }
    }
}
