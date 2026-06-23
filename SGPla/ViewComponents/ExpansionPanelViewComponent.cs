using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class ExpansionPanelViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ExpansionPanelModel model)
        {
            return View(model);
        }
    }
}