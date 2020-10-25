


using System;
using UMC.Web;

public class AccountCheckActivity : WebActivity
{



    public override void ProcessActivity(WebRequest request, WebResponse response)
    {
        var user = UMC.Security.Identity.Current;
        if (request.SendValue == "Info")
        {
            var pri = UMC.Security.Principal.Current;
            var info = new System.Collections.Hashtable();
            if (user.IsAuthenticated)
            {
                info["Alias"] = user.Alias;
                info["Src"] = UMC.Data.WebResource.Instance().ResolveUrl(user.Id.Value, "1", 4);// user.Alias;
            }
            info["IsCashier"] = request.IsCashier;
            info["TimeSpan"] = UMC.Data.Utility.TimeSpan();
            info["Device"] = UMC.Data.Utility.Guid(UMC.Security.AccessToken.Token.Value);

            var ContentType = pri.SpecificData.ContentType;
            if (String.IsNullOrEmpty(ContentType) == false)
            {
                if (ContentType.StartsWith("WeiXin"))
                {

                    info["IsWeiXin"] = true;
                }
                else if (ContentType.StartsWith("Client"))
                {

                    info["IsClient"] = true;
                }
                else if (ContentType.StartsWith("Corp"))
                {

                    info["IsCorp"] = true;
                }
            }
            response.Redirect(info);

        }
        else if (request.SendValue == "Login")
        {
            if (user.IsAuthenticated == false)
            {
                this.Context.Send("Login", true);


            }
            return;
        }
        else if (request.SendValue == "User")
        {
            if (request.IsCashier == false)
            {
                response.Redirect("Settings", "Login");


            }
        }
        else if (request.SendValue == "Client")
        {
            if (user.IsAuthenticated == false)
            {
                response.Redirect("Account", "Login");

            }
            else
            {
                response.Redirect("Account", "Self");

            }

        }
        else if (request.SendValue == "Cashier")
        {
            if (request.IsCashier == false)
            {
                response.Redirect("Settings", "Login");

            }
            else
            {
                response.Redirect("Account", "Self");

            }
        }

        if (user.IsAuthenticated == false)
        {
            if (request.SendValue == "Event")
            {
                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Login"), true);
            }
            else
            {
                response.Redirect("Account", "Login");
            }

        }
        this.Context.Send(new UMC.Web.WebMeta().UIEvent("Login", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("icon", "\uE91c", "format", "{icon}").Put("Alias", user.Alias).Put("click", new UIClick() { Command = "Self", Model = "Account" }).Put("style", new UIStyle().Name("icon", new UIStyle().Font("wdk")))), true);

    }
}
