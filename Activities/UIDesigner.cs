

using System;
using System.Collections.Generic;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
using UMC.Web.UI;
namespace UMC.Activities
{
    public class UIDesigner
    {
        bool _editer;

        public UIDesigner(bool editer)
        {
            _editer = editer;


        }

        public const int


                StoreDesignTypeCaption = 1,
                StoreDesignTypeProduct = 2,
                StoreDesignTypeBanners = 4,
                StoreDesignTypeTitleDesc = 128,
                StoreDesignTypeItems = 16,
                StoreDesignTypeIcons = 64,
                StoreDesignTypeItem = 32,
                StoreDesignTypeCustom = 256,
                StoreDesignTypeProducts = 512,
                StoreDesignTypeDiscounts = 1024,
                StoreDesignTypeDiscount = 2048,
                StoreDesignType = 2048 * 2;

        UISlider[] Sliders(Guid parentId, List<Design_Item> baners)
        {
            List<UISlider> list = new List<UISlider>();
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            foreach (Design_Item b in baners)
            {
                UISlider slider = new UISlider();

                slider.Src = webr.ImageResolve(b.Id.Value, "1", 0) + "!slider?" + TimeSpan(b.ModifiedDate);

                if (_editer)
                {
                    slider.Click = (new UIClick(new UMC.Web.WebMeta().Put("Id", b.Id)).Send("Design", "Item"));
                }
                else
                {
                    if (String.IsNullOrEmpty(b.Click) == false)
                    {
                        slider.Click = UMC.Data.JSON.Deserialize<UIClick>(b.Click);

                    }

                }
                list.Add(slider);
            }
            if (list.Count == 0 && _editer)
            {
                list.Add(new UISlider() { Click = new UIClick(parentId.ToString()).Send("Design", "Item") });

            }
            return list.ToArray();

        }

        void Sliders(Design_Item parent, List<Design_Item> baners, UISection U)
        {
            if (baners.Count > 0)
            {
                WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(parent.Data) ?? new UMC.Web.WebMeta();


                UICell slider2 = UISlider.Create(Sliders(parent.Id.Value, baners));
                int[] paddings = UIStyle.Padding(config);
                if (paddings.Length > 0)
                {
                    slider2.Style.Padding(paddings);
                }
                U.Add(slider2);
            }
            else if (_editer)
            {

                UIDesc desc = new UIDesc("\ue907");
                desc.Click(new UIClick(parent.Id.ToString())
                                    .Send("Design", "Item"));
                desc.Desc("{desc}\r\n配置横幅栏");
                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38));
                U.Add(desc);

            }
        }

        void Icons(Guid parentId, List<Design_Item> baners, UISection U)
        {
            List<UIEventText> list = new List<UIEventText>();
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            foreach (Design_Item b in baners)
            {
                UIEventText slider = new UIEventText(b.ItemName);
                if (String.IsNullOrEmpty(b.Data) == false)
                {
                    WebMeta s = UMC.Data.JSON.Deserialize<WebMeta>(b.Data);

                    slider.Icon(s.Get("icon"), s.Get("color"));

                }
                else
                {
                    slider.Src(webr.ImageResolve(b.Id.Value, "1", 4) + "?" + TimeSpan(b.ModifiedDate));

                }
                slider.Click(this.Click(b));

                list.Add(slider);

            }
            if (list.Count > 0)
            {
                U.AddIcon(new UIStyle().Name("icon", new UIStyle().Font("wdk").Size(24)), list.ToArray());
            }
            else if (_editer)
            {
                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置图标栏");
                desc.Click(new UIClick(parentId.ToString())
                        .Send("Design", "Item"));

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38));
                U.Add(desc);

            }

        }

        void Items(Design_Item parent, List<Design_Item> baners, UISection U)
        {
            Guid parentId = parent.Id.Value;
            List<UIItem> list = new List<UIItem>();
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            for (int i = 0; i < baners.Count && i < 4; i++)
            {
                Design_Item b = baners[i];
                WebMeta icon = UMC.Data.JSON.Deserialize<WebMeta>(b.Data) ?? new UMC.Web.WebMeta();
                UIItem slider = UIItem.Create(icon);
                slider.Click(this.Click(b));
                String t = "100";
                switch (baners.Count)
                {
                    case 1:
                        t = "4-1";
                        break;
                    case 2:
                        t = "2-1";
                        break;
                    case 3:
                        if (i == 0)
                        {
                            t = "2-1";
                        }
                        break;
                }

                slider.Src(String.Format("{0}!{1}?{2}", webr.ImageResolve(b.Id.Value, "1", 0), t, TimeSpan(b.ModifiedDate)));
                list.Add(slider);

            }
            if (list.Count > 0)
            {
                U.AddItems(list.ToArray());// (new UIItem[0]));
            }
            else if (_editer)
            {
                ;
                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置分块栏");

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38).Click(new UIClick(parentId.ToString())
                        .Send("Design", "Item")));
                U.Add(desc);
            }
        }


        void TitleDesc(Design_Item parent, List<Design_Item> items, UISection U)
        {

            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();


            //WebMeta config = Utility.isNull(UMC.Data.JSON.deserialize(parent.Data, WebMeta.class), new UMC.Web.WebMeta());
            WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(parent.Data) ?? new UMC.Web.WebMeta();

            int rows = UMC.Data.Utility.IntParse(config.Get("Total"), 1);
            if (rows <= 1)
            {
                int[] padding = UIStyle.Padding(config);
                foreach (Design_Item i in items)
                {
                    UICell tdesc = this.TitleDesc(config, i, "cms1", webr);
                    if (padding.Length > 0)
                        tdesc.Style.Padding(padding);
                    U.Add(tdesc);
                }
            }
            else
            {
                int m = 0;
                String hide = config.Get("Hide") ?? "";
                if (hide.Contains("HideTitle"))
                {
                    m |= 1;
                }
                if (hide.Contains("HideDesc"))
                {
                    m |= 2;
                }
                if (hide.Contains("HideLeft"))
                {
                    m |= 4;
                }
                if (hide.Contains("HideRight"))
                {
                    m |= 8;
                }

                int len = items.Count;

                for (int i = 0; (i + rows - 1) < len; i = i + rows)
                {
                    List<WebMeta> ls = new List<WebMeta>();//<>();
                    for (int c = 0; c < rows; c++)
                    {
                        UICell p = TitleDesc(config, items[i + c], "350", webr);
                        ls.Add(new UMC.Web.WebMeta().Put("value", p.Data).Put("format", p.Format).Put("style", p.Style));

                    }
                    UICell desc = UICell.Create("ItemsTitleDesc", new UMC.Web.WebMeta().Put("items", ls.ToArray()).Put("total", rows).Put("show", m));
                    int[] paddings = UIStyle.Padding(config);
                    if (paddings.Length > 0)
                    {
                        desc.Style.Padding(paddings);
                    }
                    U.Add(desc);
                }
                int total = len % rows;

                if (total > 0)
                {
                    List<WebMeta> ls = new List<WebMeta>();
                    for (int c = total; c > 0; c--)
                    {
                        UICell p = TitleDesc(config, items[len - c], "350", webr);
                        ls.Add(new UMC.Web.WebMeta().Put("value", p.Data).Put("format", p.Format).Put("style", p.Style));

                    }

                    UICell desc = UICell.Create("ItemsTitleDesc", new UMC.Web.WebMeta().Put("items", ls.ToArray()).Put("total", rows).Put("show", m));
                    int[] paddings = UIStyle.Padding(config);
                    if (paddings.Length > 0)
                    {
                        desc.Style.Padding(paddings);
                    }
                    U.Add(desc);

                }


            }
            if (items.Count == 0 && _editer)
            {

                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置图文栏");

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38).Click(new UIClick(parent.Id.ToString())
                        .Send("Design", "Item")));
                U.Add(desc);

            }
        }

        UIImageTitleDescBottom TitleDesc(WebMeta config, Design_Item item, String img, UMC.Data.WebResource webr)
        {

            //WebMeta data = Utility.isNull(UMC.Data.JSON.deserialize(item.Data, WebMeta.class), new UMC.Web.WebMeta());
            WebMeta data = UMC.Data.JSON.Deserialize<WebMeta>(item.Data) ?? new UMC.Web.WebMeta();

            int m = 0;
            String hide = config.Get("Hide") ?? "";

            if (hide.Contains("HideTitle"))
            {
                m |= 1;
                data.Remove("title");
            }
            if (hide.Contains("HideDesc"))
            {
                m |= 2;
                data.Remove("desc");
            }
            if (hide.Contains("HideLeft"))
            {
                m |= 4;
                data.Remove("left");
            }
            if (hide.Contains("HideRight"))
            {
                m |= 8;
                data.Remove("right");
            }
            data.Put("show", m);
            String src = (String.Format("{0}!{1}?{2}", webr.ImageResolve(item.Id.Value, "1", 0), img, TimeSpan(item.ModifiedDate)));
            //        list.add(slider);

            //
            UIImageTitleDescBottom btm = UIImageTitleDescBottom.Create(data, src);
            btm.Click(this.Click(item));
            var left = data["left"];
            if (String.IsNullOrEmpty(left) == false)
            {
                var p = @"\d+\.?\d{0,2}";
                int i = -1;
                var t = System.Text.RegularExpressions.Regex.Replace(left, p, dr =>
                {
                    i++;
                    switch (i)
                    {
                        case 0:
                            data.Put("price", dr.Value);
                            return "￥{1:price} ";
                        case 1:
                            data.Put("orgin", dr.Value);
                            return " {orgin}";

                    }
                    return dr.Value;
                }, System.Text.RegularExpressions.RegexOptions.Multiline);
                btm.Left(t);
                btm.Style.Name("price", new UIStyle().Size(16).Color(0xdb3652)).Name("unit", new UIStyle().Size(12).Color(0x999)).Name("orgin", new UIStyle().Color(0x999).Size(12).DelLine());

            }

    ;
            return btm;

        }

        public static UIClick Click(Design_Item item, bool editer)
        {
            if (editer)
            {

                return new UIClick(item.Id.ToString()).Send("Design", "Item");
            }
            else
            {
                return JSON.Deserialize<UIClick>(item.Click);
            }
        }
        public static int TimeSpan(DateTime? date)
        {
            return date.HasValue ? UMC.Data.Utility.TimeSpan(date.Value) : 0;
        }

        UIClick Click(Design_Item item)
        {

            return Click(item, _editer);
        }

        private UISection Section(UISection Us, List<Design_Item> items)
        {
            List<Design_Item> groups = items.FindAll(g => g.for_id == Guid.Empty);

            Design_Item b = groups.Find(g => g.Type == StoreDesignTypeBanners);


            if (b != null)
            {
                Sliders(b, groups.FindAll(it => it.for_id == b.Id), Us);
            }


            if (b != null)
            {
                groups.Remove(b);
            }
            foreach (Design_Item bp in groups)
            {
                UISection use = Us;
                if (Us.Length > 0)
                {
                    use = Us.NewSection();
                }
                switch (bp.Type ?? 0)
                {
                    case StoreDesignTypeBanners:
                        Sliders(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeIcons:
                        //Icons(bp.Id, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        Icons(bp.Id.Value, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeItems:

                        //Items(bp, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        Items(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeTitleDesc:

                        //TitleDesc(bp, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        TitleDesc(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeProducts:
                    case StoreDesignTypeDiscounts:
                        break;
                    case StoreDesignTypeCaption:

                        //WebMeta config = Utility.isNull(UMC.Data.JSON.deserialize(bp.Data, WebMeta), new UMC.Web.WebMeta());
                        WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(bp.Data) ?? new UMC.Web.WebMeta();
                        if ("Hide".Equals(config.Get("Show")))
                        {
                            if (_editer)
                            {
                                UITitleMore more = UITitleMore.Create(bp.ItemName).More("已隐藏{3:more}");
                                more.Style.Name("more", new UIStyle().Color(0xc00));

                                use.Add(more.Click(this.Click(bp)));
                            }
                        }
                        else
                        {
                            UITitleMore more = UITitleMore.Create(bp.ItemName).Click(this.Click(bp));

                            more.Style.Padding(UIStyle.Padding(config));
                            use.Add(more);
                        }
                        List<Design_Item> groups2 = items.FindAll(it => it.for_id == (bp.Id));// items.FindAll(it = > it.for_id == bp.Id);
                        foreach (Design_Item bp2 in groups2)
                        {
                            switch (bp2.Type)
                            {
                                case StoreDesignTypeBanners:
                                    Sliders(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeIcons:
                                    //Icons(bp2.Id, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    Icons(bp2.Id.Value, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeItems:
                                    //Items(bp2, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    Items(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeTitleDesc:

                                    //TitleDesc(bp2, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    TitleDesc(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                            }
                        }


                        break;


                }
            }
            return Us;
        }



        public UISection Section(String title, Guid design_id)
        {

            UISection Us = String.IsNullOrEmpty(title) ? UISection.Create() : UISection.Create(new UITitle(title));
            return this.Section(Us, design_id);
        }

        public UISection Section(UISection Us, Guid design_id)
        {
            List<Design_Item> items = new List<Design_Item>();

            Database.Instance().ObjectEntity<Design_Item>()
                    .Where.And().Equal(new Design_Item() { design_id = (design_id) })
                    .Entities.Order.Asc(new Design_Item() { Seq = 0 }).Entities
                    .Query(dr => items.Add(dr));


            return this.Section(Us, items);
        }

    }
}