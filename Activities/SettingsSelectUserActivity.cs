using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    /// <summary>
    /// 
    /// </summary>
    [Mapping("Settings", "SelectUser", Auth = WebAuthType.User, Desc = "查找用户", Category = 1)]
    public class SettingsSelectUserActivity : WebActivity
    {
        ListItem User()
        {

            var UId = Utility.Guid(this.AsyncDialog("Username", g => new UserDialog()
            {
                CloseEvent = "UI.Event",
                IsPage = true,
                Title = "选择人员",
                IsSearch = true
            })).Value;


            var user = UMC.Data.Database.Instance().ObjectEntity<User>()
                .Where.And().Equal(new User { Id = UId }).Entities.Single();
            return new ListItem(user.Alias, user.Username);
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var key = this.AsyncDialog("Key", g => this.DialogValue("Promotion"));

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, this.User()), true);
        }

    }
}
