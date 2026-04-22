using Microsoft.AspNetCore.Mvc;
using SGPla.Models;

namespace SGPla.ViewComponents
{
    public class InputFieldViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(
            string name,
            string label,
            string placeholder = "",
            string tipo = "text",
            string valor = "",
            bool disabled = false,
            string id = null)
        {
            var model = new InputFieldModel
            {
                Name = name,
                Label = label,
                Placeholder = placeholder,
                Tipo = tipo,
                Valor = valor,
                Disabled = disabled,
                Id = id ?? name
            };

            return View(model);
        }
    }
}
