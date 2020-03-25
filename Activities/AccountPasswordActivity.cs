using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Data;
using UMC.Security;
using UMC.Web;

namespace UMC.Activities
{
    class AccountPasswordActivity : WebActivity
    {


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            int type = UMC.Data.Utility.Parse(this.AsyncDialog("AccountType", g =>
            {
                return Web.UIDialog.ReturnValue("-1");
            }), 0);
            var cUser = UMC.Security.Identity.Current;
            Guid user_id = UMC.Data.Utility.Guid(this.AsyncDialog("user_id", g =>
            {
                return Web.UIDialog.ReturnValue(cUser.Id.Value.ToString());
            })) ?? Guid.Empty;

            var user = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.User>()
                .Where.And().Equal(new UMC.Data.Entities.User { Id = user_id }).Entities.Single();
            if (user == null)
            {

                type = 0;
            }

            string VerifyCode = this.AsyncDialog("VerifyCode", g =>
            {
                return Web.UIDialog.ReturnValue("0");
            });

            var Password = Web.UIFormDialog.AsyncDialog("Password", d =>
            {
                if (request.SendValues != null)
                {
                    var meta = request.SendValues;
                    if (meta.ContainsKey("NewPassword"))
                    {
                        return Web.UIDialog.ReturnValue(meta);
                    }
                }
                var dialog = new Web.UIFormDialog();

                if (type > 0)
                {
                    dialog.Title = "找回密码";
                }
                else if (type < 0)
                {
                    dialog.Title = "修改密码";
                    if (cUser.IsAuthenticated == false)
                    {
                        this.Prompt("请登录");
                    }
                    dialog.AddPassword("原密码", "Password", true);//.Put("plo")
                }
                else
                {
                    if (cUser.IsAuthenticated == false)
                    {
                        this.Prompt("请登录");
                    }
                    dialog.Title = "设置密码";
                }
                dialog.AddPassword("新密码", "NewPassword", false);
                dialog.AddPassword("确认新密码", "NewPassword2", false).Put("ForName", "NewPassword");
                dialog.Submit("确认修改", request, "account");
                return dialog;

            });
            var mc = UMC.Security.Membership.Instance();
            if (Password.ContainsKey("Password"))
            {


                if (mc.Password(cUser.Name, Password["Password"], 0) == 0)
                {
                    mc.Password(cUser.Name, Password["NewPassword"]);
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);

                    WebMeta print = new UMC.Web.WebMeta();
                    print["type"] = "account";
                    print["name"] = "Password";
                    print["value"] = "Password";
                    this.Context.Send(print, true);

                }
                else
                {
                    this.Prompt("您的原密码不正确");
                }
            }
            else
            {

                if (user == null && cUser.Id == user_id)
                {
                    Membership.Instance().CreateUser(cUser.Id.Value, cUser.Name, Password["NewPassword"], cUser.Alias);
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "account"), true);
                }

                var eac = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Account>()
                   .Where.And().Equal(new Data.Entities.Account { user_id = user_id, Type = type }).Entities.Single();

                var acc = Account.Create(eac);
                if (String.Equals(acc.Items[Account.KEY_VERIFY_FIELD] as string, VerifyCode))
                {
                    mc.Password(user.Username, Password["NewPassword"]);
                    acc.Items.Clear();
                    acc.Commit();
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "account"), true);
                }
                else
                {
                    this.Prompt("非法入侵");
                }
            }

        }

    }
}