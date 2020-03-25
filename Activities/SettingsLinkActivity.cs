using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web.UI;
using UMC.Web;
using UMC.Data;
using System.Collections;

namespace UMC.Activities
{
    class SettingsLinkActivity : Web.WebActivity
    {

        class LinkDialog : Web.UIGridDialog
        {
            protected override Hashtable GetHeader()
            {


                var header = new Header("Id", 25);
                header.AddField("Caption", "标题");
                header.AddField("Times", "点击次数");
                return header.GetHeader();


            }
            protected override Hashtable GetData(IDictionary paramsKey)
            {
                var start = UMC.Data.Utility.Parse((paramsKey["start"] ?? "0").ToString(), 0);
                var limit = UMC.Data.Utility.Parse((paramsKey["limit"] ?? "25").ToString(), 25);

                var scheduleEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Link>();
                scheduleEntity.Where.And().Equal(new Data.Entities.Link { IsMenu = true });

                string sort = paramsKey[("sort")] as string;
                string dir = paramsKey[("dir")] as string;


                if (!String.IsNullOrEmpty(sort))
                {

                    if (dir == "DESC")
                    {
                        scheduleEntity.Order.Desc(sort);
                    }
                    else
                    {
                        scheduleEntity.Order.Asc(sort);
                    }
                }
                else
                {
                    scheduleEntity.Order.Desc(new UMC.Data.Entities.Link { ModifiedTime = DateTime.MinValue });
                }

                var Keyword = (paramsKey["Keyword"] as string ?? String.Empty);//.Split(',');
                if (String.IsNullOrEmpty(Keyword) == false)
                {
                    scheduleEntity.Where.Contains().Or().Like(new Data.Entities.Link { Caption = Keyword, GroupBy = Keyword });
                }
                var count = scheduleEntity.Count();
                var hash = new Hashtable();
                hash["data"] = scheduleEntity.Query(start, limit);
                hash["total"] = count;// scheduleEntity.Count();
                if (count == 0)
                {
                    hash["msg"] = String.IsNullOrEmpty(Keyword) ? "未有搜索对应的连接配置" : String.Format("未找到相关的“{0}”连接", Keyword);
                }
                return hash;
            }
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var TypeId = UMC.Data.Utility.Guid(Web.UIDialog.AsyncDialog("Id", d =>
            {
                return new LinkDialog() { IsPage = true, Title = "连接列表" };

            }), true);

            var cateEntity = Database.Instance().ObjectEntity<UMC.Data.Entities.Link>();


            cateEntity.Where.And().Equal(new UMC.Data.Entities.Link
            {
                Id = TypeId ?? Guid.Empty
            });
            var link = cateEntity.Single() ?? new Data.Entities.Link();


            var userValue = this.AsyncDialog("User", d =>
            {
                var fdlg = new Web.UIFormDialog();
                fdlg.Title = "网页连接";

                if (link.Id.HasValue == false)
                {
                    link.Id = Guid.NewGuid();
                    request.Arguments["Id"] = link.Id.ToString();
                }
                fdlg.AddFile("图片", "_Header", Data.WebResource.Instance().ImageResolve(link.Id.Value, "1", 5))
                  .Command("Design", "Image", new UMC.Web.WebMeta().Put("id", link.Id).Put("seq", "1"));
                fdlg.AddText("网址", "Url", link.Url).Put("tip", "");
                fdlg.AddText("网址标题", "Caption", link.Caption);
                fdlg.AddText("所属组", "GroupBy", link.GroupBy);


                fdlg.Submit("确认", this.Context.Request, "Settings.Link");
                return fdlg;
            });
            UMC.Data.Reflection.SetProperty(link, userValue.GetDictionary());
            if (link.Id.HasValue)
            {
                link.ModifiedTime = DateTime.Now;
                cateEntity.Update(link);
            }
            else
            {
                link.IsMenu = true;
                link.Times = 0;
                link.CreationTime = DateTime.Now;
                link.ModifiedTime = DateTime.Now;
                link.Favs = 0;
                link.Id = TypeId;// Guid.NewGuid();
                cateEntity.Insert(link);
            }
            this.Prompt("更新成功", false);
            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Link"), true);

        }

    }
}