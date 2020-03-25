
using System;
using System.Collections.Generic;
using UMC.Web;

namespace UMC.Activities
{
    class AccountAccessActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var GroupBy = this.AsyncDialog("GroupBy", "SELF");
            if (GroupBy == "SELF")
            {
                var ids = new List<Guid>();
                UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.LinkAccess>()
                   .Order.Desc(new UMC.Data.Entities.LinkAccess { LastAccessTime = DateTime.Now })
                   .Entities.Query(0, 100, dr => ids.Add(dr.link_id.Value));
                var ls = new List<UMC.Data.Entities.Link>();
                if (ids.Count > 0)
                {

                    var lineEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Link>();
                    lineEntity.Where.And().In(new UMC.Data.Entities.Link { Id = ids[0] }, ids.ToArray());
                    lineEntity.Order.Asc(new UMC.Data.Entities.Link { CreationTime = DateTime.Now });
                    lineEntity.Query(dr => ls.Add(dr));
                }

                System.Data.DataTable data = new System.Data.DataTable();
                //data.Columns.Add("Id");
                data.Columns.Add("Caption");
                data.Columns.Add("Fav");
                data.Columns.Add("Time");
                data.Columns.Add("Href");
                data.Columns.Add("Src");
                var webr = UMC.Data.WebResource.Instance();
                foreach (var id in ids)
                {
                    var dr = ls.Find(l => l.Id == id);
                    if (dr.IsMenu == true)
                    {
                        data.Rows.Add(dr.Caption, dr.Favs, dr.Times, String.Format("#link/{0}", dr.Id), webr.ResolveUrl(dr.Id.Value, "1", "3"));

                    }
                    else
                    {
                        data.Rows.Add(dr.Caption, dr.Favs, dr.Times, dr.Url, webr.ResolveUrl(dr.Id.Value, "1", "3"));

                    }

                }
                response.Redirect(new WebMeta().Put("data", data));

            }
            else
            {

                var lineEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Link>();
                lineEntity.Where.And().Equal(new UMC.Data.Entities.Link { GroupBy = GroupBy, IsMenu = true });
                lineEntity.Order.Asc(new UMC.Data.Entities.Link { CreationTime = DateTime.Now });

                System.Data.DataTable data = new System.Data.DataTable();
                //data.Columns.Add("Id");
                data.Columns.Add("Caption");
                data.Columns.Add("Fav");
                data.Columns.Add("Time");
                data.Columns.Add("Href");
                data.Columns.Add("Src");
                var webr = UMC.Data.WebResource.Instance();

                lineEntity.Query(dr =>
                {

                    data.Rows.Add(dr.Caption, dr.Favs, dr.Times, String.Format("#link/{0}", dr.Id), webr.ResolveUrl(dr.Id.Value, "1", "3"));

                });


                response.Redirect(new WebMeta().Put("data", data));

            }

        }
    }
}