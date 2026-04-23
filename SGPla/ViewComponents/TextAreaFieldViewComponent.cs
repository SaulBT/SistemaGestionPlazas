using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class TextAreaFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TextAreaFieldModel model)
        {
            model.Id ??= model.Name;
            return View(model);
        }
    }
}