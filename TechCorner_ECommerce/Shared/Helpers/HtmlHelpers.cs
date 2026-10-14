using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechCorner_ECommerce.Helpers {
    public static class HtmlHelpers {
        public static string IsActive(this IHtmlHelper html, string controller, string action) {
            var routeData = html.ViewContext.RouteData;

            var currentController = routeData.Values["controller"]?.ToString();
            var currentAction = routeData.Values["action"]?.ToString();

            return (currentController == controller && currentAction == action) ? "active" : "";
        }

        public static string IsActiveDashboard(this IHtmlHelper html, params string[] controllers) {
            

            var currentController = html.ViewContext.RouteData.Values["controller"]?.ToString();
            return controllers.Contains(currentController) ? "active" : "";
        }
    }
}
