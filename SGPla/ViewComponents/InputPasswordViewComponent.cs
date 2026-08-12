using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class InputPasswordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(InputPasswordModel model)
        {
            return View(model);
        }
    }
}
