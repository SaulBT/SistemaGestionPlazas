using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class DatePickerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DatePickerModel model)
        {
            model.Id ??= model.Name;
            return View(model);
        }
    }
}