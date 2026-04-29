using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class RadioGroupFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(RadioGroupFieldModel model)
        {
            return View(model);
        }
    }
}