using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class CheckboxFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(CheckboxFieldModel model)
        {
            return View(model);
        }
    }
}
