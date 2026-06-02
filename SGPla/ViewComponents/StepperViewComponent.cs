using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class StepperViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(StepperModel model)
        {
            return View(model);
        }
    }
}
