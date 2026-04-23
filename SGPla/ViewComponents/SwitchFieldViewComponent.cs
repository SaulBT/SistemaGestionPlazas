using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class SwitchFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SwitchFieldModel model)
        {
            model.Id ??= model.Name;
            return View(model);
        }
    }
}