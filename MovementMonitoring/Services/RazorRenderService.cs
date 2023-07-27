using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using System.Text.Encodings.Web;

namespace MovementMonitoring.Services
{
    public interface IRazorRenderService
    {
        Task<string> ToStringAsync<T>(string viewName, T model);
    }
    public class RazorRenderService : IRazorRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IActionContextAccessor _actionContext;
        private readonly IRazorPageActivator _activator;
        public RazorRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContext,
            IRazorPageActivator activator,
            IActionContextAccessor actionContext)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _httpContext = httpContext;
            _actionContext = actionContext;
            _activator = activator;
        }
        public async Task<string> ToStringAsync<T>(string pageName, T model)
        {
            ActionContext actionContext = new(_httpContext.HttpContext!, _httpContext.HttpContext!.GetRouteData(), _actionContext.ActionContext!.ActionDescriptor);
            using StringWriter sw = new();
            RazorPageResult result = _razorViewEngine.FindPage(actionContext, pageName);
            if (result.Page == null)
            {
                throw new ArgumentNullException($"The page {pageName} cannot be found.");
            }
            RazorView view = new(_razorViewEngine,
                                 _activator,
                                 new List<IRazorPage>(),
                                 result.Page,
                                 HtmlEncoder.Default,
                                 new DiagnosticListener("RazorRenderService"));
            ViewContext viewContext = new(actionContext,
                                          view,
                                          new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                                          new TempDataDictionary(_httpContext.HttpContext!, _tempDataProvider),
                                          sw,
                                          new HtmlHelperOptions());
            IRazorPage page = (result.Page);
            page.ViewContext = viewContext;
            _activator.Activate(page, viewContext);
            await page.ExecuteAsync();
            return sw.ToString();
        }
        private IRazorPage FindPage(ActionContext actionContext, string pageName)
        {
            RazorPageResult getPageResult = _razorViewEngine.GetPage(executingFilePath: null, pagePath: pageName);
            if (getPageResult.Page != null)
            {
                return getPageResult.Page;
            }
            RazorPageResult findPageResult = _razorViewEngine.FindPage(actionContext, pageName);
            if (findPageResult.Page != null)
            {
                return findPageResult.Page;
            }
            IEnumerable<string> searchedLocations = getPageResult.SearchedLocations!.Concat(findPageResult.SearchedLocations!);
            string errorMessage = string.Join(Environment.NewLine,
                                              new[] { $"Unable to find page '{pageName}'. The following locations were searched:" }.Concat(searchedLocations));
            throw new InvalidOperationException(errorMessage);
        }
    }
}
