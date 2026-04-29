using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class SelectFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SelectFieldModel model)
        {
            model.Id ??= model.Name;
            return View(model);
        }
    }
}
