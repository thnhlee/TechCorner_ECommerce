namespace TechCorner_ECommerce.Services {
    public interface IEmailTemplateRenderer {
        Task<string> RenderAsync<TModel>(string viewPath, TModel model);
    }
}
