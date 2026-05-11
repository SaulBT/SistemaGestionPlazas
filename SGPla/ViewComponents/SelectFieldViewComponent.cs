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

            var state = ViewContext.ViewData.ModelState[model.Name];

            var selected = state?.AttemptedValue ?? model.SelectedValue;

            foreach (var opt in model.Options)
            {
                opt.Selected = opt.Value == selected;
            }

            model.Error = state?.Errors.FirstOrDefault()?.ErrorMessage;


            return View(model);
        }
    }
}
