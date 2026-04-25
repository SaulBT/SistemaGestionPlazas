using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class SelectFieldViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SelectFieldModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                model.Id = model.Name;
            }
            return View(model);
        }
    }
}
