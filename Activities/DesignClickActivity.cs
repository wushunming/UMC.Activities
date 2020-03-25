

using System;
using System.Collections;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignClickActivity : WebActivity
    {


        protected UIClick Click(UIClick ui)
        {
            String type = this.AsyncDialog("Click", g =>
            {
                UIRadioDialog di = new UIRadioDialog();
                di.Title = ("关联功能");
                ListItemCollection listItemCollection = di.Options;//new ListItemCollection();
            listItemCollection.Add("连接扫一扫", "Scanning");
                listItemCollection.Add("连接指令", "Setting");
                listItemCollection.Add("连接拨号", "Tel");
                listItemCollection.Add("连接网址", "Url");

                return di;
            });
            switch (type)
            {
                case "Scanning":
                    return UIClick.Scanning();
                case "Tel":
                    return UIClick.Tel(this.AsyncDialog("Tel", g =>
                    {
                        UITextDialog di = new UITextDialog();
                        di.Title = ("拨号号码");
                        return di;
                    }));
                case "Url":
                    return UIClick.Url(new Uri(this.AsyncDialog("Url", g =>
                    {
                        UITextDialog di = new UITextDialog();
                        di.Title = ("网址地址");
                        return di;
                    })));


                default:
                case "Setting":

                    var c = UMC.Data.JSON.Deserialize(UMC.Data.JSON.Serialize(ui)) as Hashtable;
                    WebMeta settings = this.AsyncDialog(g =>
                    {
                        UIFormDialog di = new UIFormDialog();
                        di.Title = ("功能指令");
                        di.AddText("模块指令", "Model", (String)c["model"]);
                        di.AddText("模块指令", "Command", (String)c["cmd"]);
                        di.AddPrompt("此块内容为专业内容，请由工程师设置");

                        if (c.ContainsKey("send"))
                        {
                            Object send = c["send"];
                            if (send is String)
                            {
                                di.AddText("参数", "Send", (String)send).PlaceHolder("如果没参数，则用none");
                            }
                            else
                            {

                                di.AddText("参数", "Send", UMC.Data.JSON.Serialize(send)).PlaceHolder("如果没参数，则用none");
                            }
                        }
                        else
                        {

                            di.AddText("参数", "Send", "none").PlaceHolder("如果没参数，则用none");
                        }

                        return di;
                    }, "Send");
                    UIClick click = new UIClick();
                    String Model = settings.Get("Model");
                    String Command = settings.Get("Command");
                    String Send = settings.Get("Send");
                    click.Send(Model, Command);

                    if ("none".Equals(Send, StringComparison.CurrentCultureIgnoreCase) == false)
                    {
                        if (Send.StartsWith("{"))
                        {
                            click.Send(UMC.Data.JSON.Deserialize<WebMeta>(Send));
                        }
                        else
                        {
                            click.Send(Send);
                        }
                    }
                    return click;
            }

        }

        public override void ProcessActivity(WebRequest webRequest, WebResponse webResponse)
        {

            String ssid = this.AsyncDialog("Id", "请输入ID");
            var sId = UMC.Data.Utility.Guid(ssid, true);


            var entity = Database.Instance().ObjectEntity<Design_Item>();

            entity.Where.And().Equal(new Design_Item() { Id = (sId) });


            Design_Item baner = entity.Single();

            UIClick c = UMC.Data.JSON.Deserialize<UIClick>(baner.Click) ?? new UIClick();


            entity.Update(new Design_Item() { Click = UMC.Data.JSON.Serialize(this.Click(c)) });
            this.Context.Send("Click", false);
            this.Prompt("关联成功");
        }
    }
}