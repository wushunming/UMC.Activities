using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class AccountLoginActivity : WebActivity
    {
        void SendMobileCode(string mobile)
        {

            var user = UMC.Security.Identity.Current;

            var req = this.Context.Request;

            var hask = new Hashtable();

            var session = new UMC.Configuration.Session<Hashtable>(mobile);
            if (session.ModifiedTime.AddMinutes(15) > DateTime.Now)
            {
                hask = session.Value;
            }
            else
            {
                hask["Code"] = UMC.Data.Utility.NumberCode(Guid.NewGuid().GetHashCode(), 6);
            }
            var times = UMC.Data.Utility.IntParse(String.Format("{0}", hask["Times"]), 0) + 1;
            if (times > 5)
            {
                var date = session.ModifiedTime;
                if (date.AddHours(3) > DateTime.Now)
                {
                    this.Prompt("您已经超过了5次，请您三小时后再试");
                }
                else
                {
                    times = 0;
                }
            }
            session.Commit(hask, user);


            hask["DateTime"] = DateTime.Now;

            Net.Message.Instance().Send("Login", hask, mobile);


        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var type = this.AsyncDialog("type", t => this.DialogValue("auto"));
            switch (type)
            {
                case "wx":
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "login.weixin"), true);
                    break;
                case "qq":
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "login.qq"), true);
                    break;
            }
            var user = Web.UIFormDialog.AsyncDialog("Login", d =>
            {
                if (request.SendValues != null && request.SendValues.Count > 0)
                {
                    return this.DialogValue(request.SendValues);
                }
                if (request.Url.Query.Contains("_v=Sub"))
                {
                    this.Context.Send("Login", true);
                }

                //   var u = new UMC.Data.Entities.User { Username = String.Empty };


                var dialog = new Web.UIFormDialog();
                dialog.Title = "登录";
                switch (type)
                {
                    default:
                    case "User":
                        this.Context.Send("LoginChange", false);
                        {
                            dialog.AddText("用户名", "Username", String.Empty).Put("placeholder", "用户名/手机/邮箱");

                            dialog.AddPassword("用户密码", "Password", String.Empty);

                            dialog.Submit("登录", request, "User", "LoginChange");
                            var uidesc = new UMC.Web.UI.UIDesc(new WebMeta().Put("eula", "用户协议").Put("private", "隐私政策"));
                            uidesc.Desc("登录即同意“{eula}”和“{private}”");
                            uidesc.Style.AlignCenter();
                            uidesc.Style.Color(0x888).Size(14).Height(34);
                            uidesc.Style.Name("eula").Color(0x3194d0).Click(new UIClick("365lu/provision/eula").Send("Subject", "UIData"));
                            uidesc.Style.Name("private").Color(0x3194d0).Click(new UIClick("365lu/provision/private").Send("Subject", "UIData"));
                            dialog.Add(uidesc);
                            dialog.AddUIIcon("\uf2c1", "免密登录").Command(request.Model, request.Command, "Mobile");
                            dialog.AddUIIcon("\uf1c6", "忘记密码").Put("Model", request.Model).Put("Command", "Forget");
                            dialog.AddUIIcon("\uf234", "注册新用户").Put("Model", request.Model).Put("Command", "Register");

                        }
                        break;
                    case "Mobile":
                        this.Context.Send("LoginChange", false);
                        {
                            dialog.AddText("手机号码", "Username", String.Empty).Put("placeholder", "注册的手机号码");

                            dialog.AddVerify("验证码", "VerifyCode", "您收到的验证码").Put("For", "Username").Put("To", "Mobile")
                            .Put("Command", request.Command).Put("Model", request.Model);
                            dialog.Submit("登录", request, "User", "LoginChange");

                            var uidesc = new UMC.Web.UI.UIDesc(new WebMeta().Put("eula", "用户协议").Put("private", "隐私政策"));
                            uidesc.Desc("登录即同意“{eula}”和“{private}”");
                            uidesc.Style.AlignCenter();
                            uidesc.Style.Color(0x888).Size(14).Height(34);
                            uidesc.Style.Name("eula").Color(0x3194d0).Click(new UIClick("365lu/provision/eula").Send("Subject", "UIData"));
                            uidesc.Style.Name("private").Color(0x3194d0).Click(new UIClick("365lu/provision/private").Send("Subject", "UIData"));
                            dialog.Add(uidesc);
                            dialog.AddUIIcon("\uf13e", "密码登录").Command(request.Model, request.Command, "User");
                            dialog.AddUIIcon("\uf234", "注册新用户").Command(request.Model, "Register");//.Put("Model", request.Model).Put("Command", "Register");
                        }
                        break;
                }

                return dialog;


            });

            if (user.ContainsKey("Mobile"))
            {
                var mobile = user["Mobile"];

                var account = Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Account>()
                    .Where.And().Equal(new UMC.Data.Entities.Account
                    {
                        Name = mobile,
                        Type = UMC.Security.Account.MOBILE_ACCOUNT_KEY
                    }).Entities.Single();
                if (account == null)
                {
                    this.Prompt("不存在此账户");
                }


                this.SendMobileCode(mobile);
                this.Prompt("验证码已发送", false);
                this.Context.Send(new UMC.Web.WebMeta().UIEvent("VerifyCode", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("text", "验证码已发送")), true);
            }

            var username = user["Username"];

            var userManager = UMC.Security.Membership.Instance();
            if (user.ContainsKey("VerifyCode"))
            {
                var VerifyCode = user["VerifyCode"];
                var session = new UMC.Configuration.Session<Hashtable>(username);
                if (session.Value != null)
                {
                    var code = session.Value["Code"] as string;
                    if (String.Equals(code, VerifyCode) == false)
                    {
                        this.Prompt("请输入正确的验证码");
                    }
                }
                else
                {
                    this.Prompt("请输入正确的验证码");

                }
                var entity = Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Account>();
                UMC.Data.Entities.Account ac = new UMC.Data.Entities.Account
                {
                    Name = username,
                    Type = UMC.Security.Account.MOBILE_ACCOUNT_KEY
                };

                var eData = entity.Where.And().Equal(ac).Entities.Single();
                if (eData == null)
                {

                    this.Prompt("无此号码关联的账户，请注册");
                }
                else
                {
                    var iden = userManager.Identity(eData.user_id.Value);
                    //System.Security.Principal.IPrincipal p = iden;
                    //if (p.IsInRole(UMC.Security.Membership.UserRole))
                    //{
                    //    this.Prompt("您是内部账户，不可从此入口登录");
                    //}


                    UMC.Security.AccessToken.Login(iden, UMC.Security.AccessToken.Token.Value, request.IsApp ? "App" : "Client", true);
                    this.Context.Send("User", true);
                }
            }
            else
            {
                var passwork = user["Password"];

                var maxTimes = 5;
                UMC.Security.Identity identity = null;
                if (UMC.Data.Utility.IsPhone(username))
                {
                    identity = userManager.Identity(username, Security.Account.MOBILE_ACCOUNT_KEY) ?? userManager.Identity(username);
                }
                else if (username.IndexOf('@') > -1)
                {
                    identity = userManager.Identity(username, Security.Account.EMAIL_ACCOUNT_KEY) ?? userManager.Identity(username);
                }
                else
                {
                    identity = userManager.Identity(username);
                }
                if (identity == null)
                {
                    this.Prompt("用户不存在，请确认用户名");
                }
                var times = userManager.Password(identity.Name, passwork, maxTimes);
                switch (times)
                {
                    case 0:
                        var iden = userManager.Identity(username);
                        //System.Security.Principal.IPrincipal p = iden;
                        //if (p.IsInRole(UMC.Security.Membership.UserRole))
                        //{
                        //    this.Prompt("您是内部账户，不可从此入口登录");
                        //}


                        UMC.Security.AccessToken.Login(iden, UMC.Security.AccessToken.Token.Value, request.IsApp ? "App" : "Client", true);


                        this.Context.Send("User", true);



                        break;
                    case -2:
                        this.Prompt("您的用户已经锁定，请过后登录");
                        break;
                    case -1:
                        this.Prompt("您的用户不存在，请确定用户名");

                        break;
                    default:
                        this.Prompt(String.Format("您的用户和密码不正确，您还有{0}次机会", maxTimes - times));

                        break;
                }
            }
        }

    }
}