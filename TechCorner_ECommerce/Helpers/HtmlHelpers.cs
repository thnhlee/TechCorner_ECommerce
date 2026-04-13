using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechCorner_ECommerce.Helpers {
    public static class HtmlHelpers {
        public static string IsActive(this IHtmlHelper html, string controller, string action) {
            var routeData = html.ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString();
            var currentAction = routeData.Values["action"]?.ToString();

            return (currentController == controller && currentAction == action) ? "active" : "";
        }
    }
}
