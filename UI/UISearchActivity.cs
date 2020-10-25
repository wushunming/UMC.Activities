using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    class UISearchActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;
            var form = request.SendValues ?? new UMC.Web.WebMeta();

            if (form.ContainsKey("limit"))
            {

                UISection ui = UISection.Create();


                var hot = new UMC.Web.UI.UITextItems();
                ui.NewSection().Add(hot).Header.Put("text", "热门搜索");

                var history = new UMC.Web.UI.UITextItems(request.Model, request.Command);
                history.Event("SearchFor");
                ui.NewSection().Add(history).Header.Put("text", "历史搜索");

                var entity = Data.Database.Instance().ObjectEntity<SearchKeyword>()
                                 .Where
                                 .And().In(new SearchKeyword { user_id = Guid.Empty })
                                 .Entities.Order.Desc(new SearchKeyword { Time = 0 }).Entities;
                entity.Query(0, 20, dr => hot.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));


                response.Redirect(ui);
            }

            if (String.IsNullOrEmpty(request.SendValue))
            {

                var history = new List<UIEventText>();

                var entity = Data.Database.Instance().ObjectEntity<SearchKeyword>()
                                 .Where
                                 .And().In(new SearchKeyword { user_id = user.Id })
                                 .Entities.Order.Desc(new SearchKeyword { Time = 0 }).Entities;
                entity.Query(0, 20, dr => history.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));


                var hash = new System.Collections.Hashtable();

                hash["data"] = history;
                if (history.Count == 0)
                {
                    hash["msg"] = "请搜索";
                }
                response.Redirect(hash);
            }
            else
            {
                var vs = request.SendValue.Split(',', ' ', '　');
                var entity = Data.Database.Instance().ObjectEntity<SearchKeyword>()
                                 .Order.Desc(new SearchKeyword { Time = 0 }).Entities;
                var list = new List<SearchKeyword>();

                foreach (var i in vs)
                {
                    if (String.IsNullOrEmpty(i) == false)
                    {
                        var search = new SearchKeyword { Keyword = i, user_id = user.Id, Time = UMC.Data.Utility.TimeSpan() };
                        entity.Where.Reset().And().Equal(new SearchKeyword { Keyword = i, user_id = user.Id });
                        if (entity.Update(new SearchKeyword { Time = UMC.Data.Utility.TimeSpan() }) == 0)
                        {
                            entity.Insert(search);
                        }
                        entity.Where.Reset().And().Equal(new SearchKeyword { Keyword = i, user_id = Guid.Empty });
                        if (entity.Update("{0}+{1}", new SearchKeyword { Time = 1 }) == 0)
                        {
                            search.Time = 1;
                            search.user_id = Guid.Empty;
                            entity.Insert(search);
                        }
                    }
                }

                var history = new List<UIEventText>();
                entity.Where.Reset().And().Equal(new SearchKeyword { user_id = user.Id });
                entity.Query(0, 20, dr => history.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));
                var hash = new System.Collections.Hashtable();
                hash["data"] = history;
                response.Redirect(hash);

            }
            //var data= new System.Data.datat
            //.Query(0, 100, dr => products.Add(dr));

        }

    }
}