using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class ModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ModalModel model) { return View(model); }
    }
}
