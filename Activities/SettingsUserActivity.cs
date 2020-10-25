using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Web;

namespace UMC.Activities
{

    class SettingsUserActivity : WebActivity
    {

        void Setting(Guid userId)
        {
            var userEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.User>();
            var user = userEntity.Where.And().Equal(new Data.Entities.User { Id = userId }).Entities.Single();
            var userRoleEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.UserToRole>();
            userRoleEntity.Where.And().Equal(new Data.Entities.UserToRole { user_id = user.Id });
            var roleEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Role>();
            roleEntity.Where.And().Unequal(new Data.Entities.Role { Rolename = UMC.Security.Membership.GuestRole });
            var userValue = Web.UIFormDialog.AsyncDialog("User", d =>
            {
                var fdlg = new Web.UIFormDialog();
                fdlg.Menu("角色", "Settings", "Role");
                fdlg.Title = String.Format("账户设置", user.Username);

                var opts2 = new Web.ListItemCollection();

                opts2.Add("别名", user.Alias);
                opts2.Add("登录名", user.Username);
                if (user.ActiveTime.HasValue)
                    opts2.Add("最后登录", String.Format("{0:yy-MM-dd HH:mm}", user.ActiveTime));
                if (user.RegistrTime.HasValue)
                    opts2.Add("注册时间", String.Format("{0:yy-MM-dd HH:mm}", user.RegistrTime));

                fdlg.AddTextValue(opts2);
                var flags = user.Flags ?? UMC.Security.UserFlags.Normal;
                var opts = new Web.ListItemCollection();
                var selected = ((int)(flags & UMC.Security.UserFlags.Lock)) > 0;
                opts.Add("锁定", "1", selected);
                selected = ((int)(flags & UMC.Security.UserFlags.Disabled)) > 0;
                opts.Add("禁用", "16", selected);
                fdlg.AddCheckBox("状态", "Flags", opts, "0");

                var uRs = new List<UMC.Data.Entities.UserToRole>(userRoleEntity.Query());
                opts = new Web.ListItemCollection();



                roleEntity.Query(dr =>
                {
                    switch (dr.Rolename)
                    {
                        case UMC.Security.Membership.AdminRole:
                            opts.Add("超级管理员", dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;
                        case UMC.Security.Membership.UserRole:
                            opts.Add("内部员工", dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;
                        case "TrainAdmin":
                            opts.Add("培训管理员", dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;


                        case "StoreManager":
                            opts.Add("店长", dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;
                        case "Finance":
                            opts.Add("财务", dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;
                        default:
                            opts.Add(dr.Rolename, dr.Id.ToString(), uRs.Exists(ur => ur.role_id == dr.Id));
                            break;
                    }
                });

                fdlg.AddCheckBox("部门角色", "Roles", opts, "None");
                return fdlg;
            });
            var Flags = UMC.Security.UserFlags.Normal;
            foreach (var k in userValue["Flags"].Split(','))
            {
                Flags = Flags | UMC.Data.Utility.Parse(k, UMC.Security.UserFlags.Normal);
            }
            userEntity.Update(new Data.Entities.User { Flags = Flags, Alias = userValue["Alias"] });

            if ((Flags & Security.UserFlags.Disabled) == Security.UserFlags.Disabled)
            {
                UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Session>()
                    .Where.And().Equal(new Data.Entities.Session
                    {
                        user_id = user.Id
                    }).Entities.Delete();
            }

            var rids = new List<Data.Entities.UserToRole>();
            foreach (var k in userValue["Roles"].Split(','))
            {
                switch (k)
                {
                    case "None":
                        break;
                    default:
                        rids.Add(new Data.Entities.UserToRole { role_id = new Guid(k), user_id = userId });
                        break;
                }
            }
            userRoleEntity.Delete();
            if (rids.Count > 0)
            {
                userRoleEntity.Insert(rids.ToArray());

                UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Session>()
                    .Where.And().Equal(new Data.Entities.Session { user_id = userId }).Entities.Delete();
            }


            UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Session>()
                 .Where.And().Equal(new Data.Entities.Session { user_id = userId }).Entities.Delete();

            this.Prompt("设置成功");
        }


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var strUser = Web.UIDialog.AsyncDialog("Id", d =>
            {
                var dlg = new UserDialog();
                dlg.IsSearch = true;
                dlg.IsPage = true;
                if (request.IsMaster)
                {
                    dlg.Menu("创建", "Settings", "User", Guid.Empty.ToString());
                }
                dlg.RefreshEvent = "Setting";
                return dlg;
            });
            var userId = UMC.Data.Utility.Guid(strUser) ?? Guid.Empty;
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理账户");
            }


            var userEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.User>();
            var user = userEntity.Where.And().Equal(new Data.Entities.User
            {
                Id = userId
            }).Entities.Single() ?? new Data.Entities.User();

            var isAliassetting = false;
            if (userId != Guid.Empty && user.Id.HasValue)
            {

                var setting = Web.UIDialog.AsyncDialog("Setting", d =>
                {
                    var frm = new Web.UIRadioDialog();
                    frm.Title = "用户操作";
                    frm.Options.Add("部门角色", "Setting");
                    frm.Options.Add("重置密码", "Passwrod");
                    frm.Options.Add("变更别名", "Alias");

                    if (Web.WebServlet.Auths().Count > 0)
                    {
                        frm.Options.Add("功能授权", "Auth");
                    }
                    return frm;
                });
                switch (setting)
                {
                    case "Setting":
                        this.Setting(userId);
                        break;
                    case "Auth":
                        response.Redirect("Settings", "Auth", new UMC.Web.WebMeta().Put("Type", "User", "Value", user.Username), true);
                        break;
                    case "Alias":
                        isAliassetting = true;
                        break;
                }
            }

            var users = this.AsyncDialog("User", d =>
            {
                var opts = new Web.ListItemCollection();
                var fmDg = new Web.UIFormDialog();
                if (userId == Guid.Empty || user.Id.HasValue == false)
                {
                    fmDg.Title = "添加新账户";

                    fmDg.AddText("账户名", "Username", String.Empty);
                    fmDg.AddText("别名", "Alias", user.Alias);

                    fmDg.AddPassword("密码", "Password", true);

                }
                else
                {
                    if (isAliassetting)
                    {
                        fmDg.Title = "变更别名";
                        opts.Add("登录名", user.Username);
                        fmDg.AddText("新别名", "Alias", user.Alias);
                    }
                    else
                    {
                        fmDg.Title = "重置密码";
                        opts.Add("别名", user.Alias);
                        opts.Add("登录名", user.Username);
                        fmDg.AddTextValue(opts);
                        fmDg.AddPassword("密码", "Password", true);
                    }

                }
                fmDg.Submit("确认提交", request, "Setting");
                return fmDg;
            });

            if (userId == Guid.Empty || user.Id.HasValue == false)
            {
                if (userId == Guid.Empty)
                {
                    userId = UMC.Security.Membership.Instance().CreateUser(users["Username"].Trim(), users["Password"] ?? Guid.NewGuid().ToString(), users["Alias"]);
                    if (userId == Guid.Empty)
                    {
                        this.Prompt(String.Format("已经存在{0}用户名", users["Username"]));
                    }
                    else
                    {
                        UMC.Security.Membership.Instance().AddRole(users["Username"].Trim(), UMC.Security.Membership.UserRole);
                    }

                }
                else
                {
                    var uid = UMC.Security.Membership.Instance().CreateUser(userId, users["Username"].Trim(), users["Password"] ?? Guid.NewGuid().ToString(), users["Alias"]);
                    if (uid == null)
                    {
                        this.Prompt(String.Format("已经存在{0}用户名", users["Username"]));
                    }
                }


                this.Prompt("账户添加成功", false);

                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Setting"), true);

            }
            else
            {
                if (users.ContainsKey("Password"))
                {
                    UMC.Security.Membership.Instance().Password(user.Username, users["Password"]);
                    this.Prompt(String.Format("{0}的密码已重置", user.Alias), false);
                }
                else
                {
                    UMC.Security.Membership.Instance().ChangeAlias(user.Username, users["Alias"]);
                    this.Prompt(String.Format("{0}的别名已重置成{1}", user.Username, users["Alias"]), false);
                }

                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Setting"), true);
            }

        }

    }
}