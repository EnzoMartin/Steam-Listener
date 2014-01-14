using System.Web;
using System.Web.Optimization;

namespace Steam_Listener
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/js").Include("~/Scripts/jquery.signalR-2.0.1.min,js"));
            
        }
    }
}