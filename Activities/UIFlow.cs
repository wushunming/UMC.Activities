using System;
using System.Collections.Generic;
using System.Text;
using UMC.Web;

namespace UMC.Activities
{
    [Mapping("UI", Auth = WebAuthType.User, Desc = "UI页面")]
    class UIFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {

                case "App":
                    return new UIAppActivity();
                case "Home":
                    return new DesignUIActivity(false);
                default:
                    return WebActivity.Empty;

            }

        }

    }
}
