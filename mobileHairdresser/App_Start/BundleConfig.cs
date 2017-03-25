using System.Web;
using System.Web.Optimization;

namespace mobileHairdresser
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //JS scripts to allow for jquery to be used on the website.
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //JS scripts to allow bootstrap functions on the website.
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            //JS script generated to allow custom functions on the website to work.
            bundles.Add(new ScriptBundle("~/bundles/customScript").Include(
                "~/Scripts/Site.js"));

            //JS scripts for script libarys
            bundles.Add(new ScriptBundle("~/bundles/StyleScripts").Include(
                "~/Scripts/dropzone/dropzone.js"));

            //Groups all css pages 
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/Content/themes/base/jquery-ui.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/Content.css",
                      "~/Scripts/dropzone/dropzone.min.css"));
        }
    }
}
