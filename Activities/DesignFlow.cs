
using UMC.Web;

namespace UMC.Activities
{

    [Mapping("Design", Auth = WebAuthType.User, Desc = "UI…Ëº∆")]
    class DesignFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "Banner":
                    return new DesignBannerActivity();
                case "Item":
                    return new DesignItemActivity();
                case "Click":
                    return new DesignClickActivity();
                case "Custom":
                    return new DesignCustomActivity();
                case "Items":
                    return new DesignItemsActivity();
                case "Image":
                    return new DesignImageActivity();
                case "Page":
                    return new DesignUIActivity();
                default:
                    if (this.Context.Request.Command.StartsWith("UI"))
                    {
                        return new DesignConfigActivity();
                    }
                    return WebActivity.Empty;

            }

        }
    }
}
