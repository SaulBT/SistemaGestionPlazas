using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class ModalArchivoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ModalArchivoModel model) { return View(model); }
    }
}
