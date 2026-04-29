using Microsoft.AspNetCore.Mvc;

namespace SGPla.ViewComponents
{
    public class ModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ModalModel model) { return View(model); }
    }
}
