using Foundation;
using UIKit;

namespace InventoryApp
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        /// <inheritdoc />
        public override void WillTerminate(UIApplication application)
        {
            ExitBackupHelper.RunOnExit();
            base.WillTerminate(application);
        }
    }
}
