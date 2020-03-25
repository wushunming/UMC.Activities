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
    class SettingsMenuActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var TypeId = UMC.Data.Utility.Guid(Web.UIDialog.AsyncDialog("Id", dkey =>
            {
                var menus = new List<UMC.Data.Entities.Menu>();
                UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Menu>().Order.Asc(new UMC.Data.Entities.Menu { Seq = 0 })
                .Entities.Query(dr => menus.Add(dr));
                //menubar

                var menu = new List<WebMeta>();

                foreach (var p in menus.FindAll(d => d.ParentId == Guid.Empty))
                {
                    var IsDisable = p.IsDisable == true;
                    var m = new WebMeta().Put("icon", p.Icon).Put("text", p.Caption).Put("id", p.Id).Put("disable", p.IsDisable == true);

                    //m.Put("url")
                    menu.Add(m);
                    var data2 = new System.Data.DataTable();
                    data2.Columns.Add("id");
                    data2.Columns.Add("text");
                    data2.Columns.Add("url");
                    data2.Columns.Add("disable", typeof(bool));
                    var childs = menus.FindAll(c => c.ParentId == p.Id);
                    if (childs.Count > 0)
                    {
                        foreach (var ch in childs)
                        {
                            data2.Rows.Add(ch.Id, ch.Caption, ch.Url, IsDisable || ch.IsDisable == true);
                        }

                        m.Put("menu", data2);
                    }
                    else
                    {
                        m.Put("url", p.Url);
                    }
                }

                response.Redirect(menu);
                return this.DialogValue("none");

            }), true);

            var cateEntity = Database.Instance().ObjectEntity<UMC.Data.Entities.Menu>();


            cateEntity.Where.And().Equal(new UMC.Data.Entities.Menu
            {
                Id = TypeId ?? Guid.Empty
            });
            var link = cateEntity.Single() ?? new Data.Entities.Menu();

            var parentId = link.ParentId ?? UMC.Data.Utility.Guid(this.AsyncDialog("ParentId", "none")) ?? Guid.Empty;


            var userValue = this.AsyncDialog("Settings", d =>
            {
                var fdlg = new Web.UIFormDialog();
                fdlg.Title = "菜单设置";

                if (parentId == Guid.Empty)
                {
                    fdlg.AddOption("菜单图标", "Icon", link.Icon, String.IsNullOrEmpty(link.Icon) ? "请选择" : "已选择").PlaceHolder("请参考UMC图标库")
                    .Command("System", "Icon");

                }
                fdlg.AddText("菜单标题", "Caption", link.Caption);
                if (parentId == Guid.Empty)
                {
                    fdlg.AddText("菜单网址", "Url", link.Url).NotRequired();
                }
                else
                {

                    fdlg.AddText("菜单网址", "Url", link.Url);//.Put("tip", "");
                }
                fdlg.AddNumber("展示顺序", "Seq", link.Seq);
                if (link.Id.HasValue)
                {
                    fdlg.AddCheckBox("", "Status", "n").Add("禁用此菜单", "Disable", link.IsDisable == true);
                    fdlg.AddUIIcon(UIIcon.fa_lock, "权限设置").Command(request.Model, "Auth", link.Id.ToString());
                }


                fdlg.Submit("确认", this.Context.Request, "Settings.Menu");
                return fdlg;
            });
            UMC.Data.Reflection.SetProperty(link, userValue.GetDictionary());
            if (link.Id.HasValue)
            {
                link.IsDisable = (userValue["Status"] ?? "").Contains("Disable");
                cateEntity.Update(link);
                this.Prompt("更新成功", false);
            }
            else
            {
                link.ParentId = parentId;
                link.Id = Guid.NewGuid();
                link.IsDisable = false;
                cateEntity.Insert(link);
                this.Prompt("添加成功", false);
            }
            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Menu"), true);

        }

    }
}