using Microsoft.AspNetCore.Mvc;
using SGPla.Models.Components;

namespace SGPla.ViewComponents
{
    public class TabsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TabsModel model)
        {
            return View(model);
        }
    }
}