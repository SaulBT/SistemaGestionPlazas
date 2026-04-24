using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class InputFieldViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(InputFieldModel model)
        {
            model.Id ??= model.Name;
            model.Tipo ??= "text";
            return View(model);
        }
    }
}
