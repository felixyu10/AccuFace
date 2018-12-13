
using System.Web.Optimization;

namespace FaceAPI.Web
{
    public class BundleConfig
    {
        // 如需 Bundling 的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/base/js").Include(
                                        "~/Scripts/jquery/jquery-1.8.3.min.js",
                                        "~/Scripts/jquery/jquery.validate.min.js",
                                        "~/Scripts/jquery/jquery.validate.unobtrusive.min.js",
                                        "~/Scripts/modernizr-2.6.2.js",
                                        "~/Scripts/bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                                        "~/Content/Site.css",
                                        "~/Content/bootstrap.min.css"));
        }
    }
}
