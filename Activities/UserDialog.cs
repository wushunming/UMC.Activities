
using System;
using System.Collections;

namespace UMC.Activities
{
    public class UserDialog : UMC.Web.UIGridDialog
    {
        public UserDialog()
        {
            this.Title = "账户管理";

        }
        protected override Hashtable GetHeader()
        {


            var header = new Header("Id", 25);
            header.AddField("Username", "登录名");
            header.AddField("Alias", "别名");
            return header.GetHeader();


        }
        protected override Hashtable GetData(IDictionary paramsKey)
        {
            var start = UMC.Data.Utility.Parse((paramsKey["start"] ?? "0").ToString(), 0);
            var limit = UMC.Data.Utility.Parse((paramsKey["limit"] ?? "25").ToString(), 25);
            //var filter = new WebADNuke.Utils.FormFilter(paramsKey);

            var scheduleEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.User>();

            string sort = paramsKey[("sort")] as string;
            string dir = paramsKey[("dir")] as string;


            if (!String.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Disabled":
                        scheduleEntity.Where.And("(Flags&{0})={0}", UMC.Security.UserFlags.Disabled);
                        break;
                    case "Lock":
                        scheduleEntity.Where.And("(Flags&{0})={0}", UMC.Security.UserFlags.Lock);
                        break;

                    default:
                        if (dir == "DESC")
                        {
                            scheduleEntity.Order.Desc(sort);
                        }
                        else
                        {
                            scheduleEntity.Order.Asc(sort);
                        }
                        break;
                }
            }
            else
            {
                scheduleEntity.Order.Desc(new UMC.Data.Entities.User { RegistrTime = DateTime.MinValue });
            }

            System.Data.DataTable data = new System.Data.DataTable();
            data.Columns.Add("Id");
            data.Columns.Add("Username");
            data.Columns.Add("Alias");
            data.Columns.Add("RegistrTime");

            var Keyword = (paramsKey["Keyword"] as string ?? String.Empty);//.Split(',');
            if (String.IsNullOrEmpty(Keyword) == false)
            {
                scheduleEntity.Where.Contains().Or().Like(new UMC.Data.Entities.User { Username = Keyword, Alias = Keyword });
            }

            scheduleEntity.Query(start, limit, dr =>
            {

                data.Rows.Add(dr.Id, dr.Username, dr.Alias, UMC.Data.Utility.GetDate(dr.RegistrTime));

            });

            var hash = new Hashtable();
            hash["data"] = data;
            hash["total"] = scheduleEntity.Count();
            return hash;
        }
    }
}