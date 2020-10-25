﻿using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Web.UI;
using UMC.Web;
using UMC.Activities.Entities;

namespace UMC.Activities
{
    class DesignBannerActivity : UMC.Web.WebActivity
    {
        bool _editer;
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;
            this._editer = request.IsCashier;

            var designId = UMC.Data.Utility.Guid(this.AsyncDialog("Id", g => new Web.UITextDialog()), true).Value;
            if (request.SendValues == null)
            {
                var builder2 = new UIDataSource(request.Model, request.Command, new UMC.Web.WebMeta().Put("Id", designId), "CMSImage");

                var item = Database.Instance().ObjectEntity<Design_Item>()
                       .Where.And().Equal(new Design_Item { design_id = designId, Type = UIDesigner.StoreDesignTypeBanners, for_id = Guid.Empty }).Entities.Single();
                if (item == null)
                {
                    item = new Design_Item { Id = Guid.NewGuid(), Type = UIDesigner.StoreDesignTypeBanners, for_id = Guid.Empty, design_id = designId };
                    Database.Instance().ObjectEntity<Design_Item>().Insert(item);
                }

                this.Context.Send(new UMC.Web.WebMeta().Put("type", "DataSource").Put("title", "广告图").Put("menu", new object[] { new UIClick(new UMC.Web.WebMeta().Put("Id", item.Id.ToString(), "Type", "Banners")) { Command = "Design", Model = "Item", Text = "新建" } }).Put("DataSource", new object[] { builder2 }).Put("model", "Cells").Put("RefreshEvent", "Design"), true);


            }



            var items = new List<Design_Item>();


            var itemEntity = Database.Instance().ObjectEntity<Design_Item>()
                   .Where.And().Equal(new Design_Item { design_id = designId }).Entities;
            itemEntity.Order.Asc(new Design_Item { Seq = 0 }).Entities.Query(dr =>
            {

                items.Add(dr);
            });
            if (items.Count == 0 && this._editer)
            {
                items.Add(new Design_Item { Id = Guid.NewGuid(), Type = UIDesigner.StoreDesignTypeBanners, for_id = Guid.Empty, design_id = designId });
                itemEntity.Insert(items.ToArray());
            }
            var groups = items.FindAll(g => g.for_id == Guid.Empty);
            var parent = groups.Find(g => g.Type == UIDesigner.StoreDesignTypeBanners) ?? new Design_Item() { Id = Guid.NewGuid() };

            var list = new List<Object>();

            var webr = UMC.Data.WebResource.Instance();
            foreach (var b in items.FindAll(g => g.for_id == parent.Id))
            {
                var slider = new WebMeta().Put("src", webr.ResolveUrl(String.Format("{1}{0}/1/0.jpg!slider", b.Id, UMC.Data.WebResource.ImageResource)));
                if (_editer)
                {
                    slider.Put("click", new UIClick(new UMC.Web.WebMeta().Put("Id", b.Id)) { Command = "Design", Model = "Item" });
                }
                else
                {
                    if (String.IsNullOrEmpty(b.Click) == false)
                    {

                        slider.Put("click", UMC.Data.JSON.Deserialize<UIClick>(b.Click));
                    }

                }
                list.Add(slider);
            }

            response.Redirect(new UMC.Web.WebMeta().Put("data", list));


        }


    }
}