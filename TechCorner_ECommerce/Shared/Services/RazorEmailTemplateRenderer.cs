using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace TechCorner_ECommerce.Services {
    public class RazorEmailTemplateRenderer : IEmailTemplateRenderer {
        private readonly IRazorViewEngine viewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IServiceProvider serviceProvider;

        public RazorEmailTemplateRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider) {
            this.viewEngine = viewEngine;
            this.tempDataProvider = tempDataProvider;
            this.httpContextAccessor = httpContextAccessor;
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> RenderAsync<TModel>(string viewPath, TModel model) {
            var httpContext = httpContextAccessor.HttpContext ?? new DefaultHttpContext {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            var viewResult = viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: false);

            if (!viewResult.Success) {
                viewResult = viewEngine.FindView(actionContext, viewPath, isMainPage: false);
            }

            if (!viewResult.Success) {
                var searchedLocations = string.Join(Environment.NewLine, viewResult.SearchedLocations ?? Enumerable.Empty<string>());
                throw new InvalidOperationException($"Email template '{viewPath}' was not found. Searched locations:{Environment.NewLine}{searchedLocations}");
            }

            await using var writer = new StringWriter();

            var viewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary()) {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                new TempDataDictionary(httpContext, tempDataProvider),
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }
    }
}
