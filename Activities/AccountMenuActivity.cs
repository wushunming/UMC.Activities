
using System;
using System.Collections.Generic;
using UMC.Web;

namespace UMC.Activities
{
    class AccountMenuActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var ids = new List<String>();
            var menus = new List<UMC.Data.Entities.Menu>();
            UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Menu>().Where.And().Equal(new Data.Entities.Menu
            {
                IsDisable = false
            }).Entities.Order.Asc(new UMC.Data.Entities.Menu { Seq = 0 })
            .Entities.Query(dr =>
            {
                ids.Add(dr.Id.ToString());
                menus.Add(dr);
            });
            var auths = Security.AuthManager.IsAuthorization(ids.ToArray());
            var menu = new List<WebMeta>();

            foreach (var p in menus.FindAll(d => d.ParentId == Guid.Empty))
            {
                var m = new WebMeta().Put("icon", p.Icon).Put("text", p.Caption).Put("id", p.Id);
                if (auths[ids.IndexOf(p.Id.ToString())] == false)
                {
                    continue;

                }
                var data2 = new System.Data.DataTable();
                data2.Columns.Add("id");
                data2.Columns.Add("text");
                data2.Columns.Add("url");
                var childs = menus.FindAll(c => c.ParentId == p.Id);
                if (childs.Count > 0)
                {
                    childs = childs.FindAll(d => auths[ids.IndexOf(d.Id.ToString())]);
                    if (childs.Count == 0)
                    {
                        continue;
                    }
                    foreach (var ch in childs)
                    {
                        data2.Rows.Add(ch.Id, ch.Caption, ch.Url);
                    }

                    m.Put("menu", data2);
                }
                else
                {
                    m.Put("url", p.Url);
                }
                menu.Add(m);
            }

            response.Redirect(menu);
        }
    }
}