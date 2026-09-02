using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

public class TimeFieldViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(TimeFieldModel model)
    {
        model.Id ??= model.Name;
        return View(model);
    }
}