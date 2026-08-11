using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class DatePickerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DatePickerModel model)
        {
            model.Id ??= model.Name;

            var state = ViewContext.ViewData.ModelState[model.Name];
            model.Error = state?.Errors.FirstOrDefault()?.ErrorMessage ?? model.Error;

            return View(model);
        }
    }
}