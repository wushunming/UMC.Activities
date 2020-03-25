

using System;
using UMC.Web;

namespace UMC.Activities
{
    public class SettingsRoleActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var strUser = this.AsyncDialog("Id", d =>
            {
                var dlg = new RoleDialog();
                dlg.Title = "角色管理";
                dlg.IsPage = true;
                dlg.RefreshEvent = "Role";
                if (request.IsMaster)
                {

                    dlg.Menu("新建", "Settings", "User", new UMC.Web.WebMeta().Put("Id", "News"));
                }
                return dlg;
            });
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理账户角色");
            }

            var userId = UMC.Data.Utility.Guid(strUser) ?? Guid.Empty;

            var userEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Role>();
            var user = userEntity.Where.And().Equal(new UMC.Data.Entities.Role
            {
                Id = userId
            }).Entities.Single() ?? new UMC.Data.Entities.Role();

            var setting = this.AsyncDialog("Setting", d =>
            {
                var frm = new UIFormDialog();
                frm.Title = "用户角色";
                frm.AddText("角色名", "Rolename", user.Rolename);
                frm.AddTextarea("角色说明", "Explain", user.Explain).Put("tip", "角色说明");

                return frm;
            });
            if (user.Id.HasValue)
            {
                userEntity.Update(new UMC.Data.Entities.Role
                {
                    Rolename = setting["Rolename"],
                    Explain = setting["Explain"]
                });
            }
            else
            {
                userEntity.Insert(new UMC.Data.Entities.Role
                {
                    Id = Guid.NewGuid(),
                    Rolename = setting["Rolename"],
                    Explain = setting["Explain"]
                });
            }


            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Role"), true);
        }
    }


}