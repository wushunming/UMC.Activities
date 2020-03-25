
using System;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignImageActivity : WebActivity
    {


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;// ();
            Guid groupId = UMC.Data.Utility.Guid(this.AsyncDialog("id", d =>
            {
                this.Prompt("请传入参数");
                return this.DialogValue(user.Id.ToString());
            }), true).Value;

            String Seq = this.AsyncDialog("seq", g =>
            {
                if (request.SendValues != null)
                {

                    return this.DialogValue(request.SendValues.Get("Seq") ?? "0");
                }
                else
                {
                    return this.DialogValue("0");
                }
            });
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            String media_id = this.AsyncDialog("media_id", g =>
            {
                if (request.IsApp)
                {
                    UIDialog f = UIDialog.CreateDialog("File");

                    f.Config.Put("Submit", new UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(g, "Value"))
                            .Send(request.Model, request.Command));
                    ;
                    return f;

                }
                else
                {

                    UIFormDialog from = new UIFormDialog();
                    from.Title = ("图片上传");

                    from.AddFile("选择图片", "media_id", webr.ImageResolve(groupId, Seq, 4));

                    from.Submit("确认上传", request, "image");
                    return from;
                }
            });


            String type = this.AsyncDialog("type", g => this.DialogValue("jpg"));
            int seq = UMC.Data.Utility.Parse(Seq, 1);
            if (media_id.StartsWith("http://") || media_id.StartsWith("https://"))
            {
                Uri url = new Uri(media_id);

                if (url.AbsolutePath.ToLower().EndsWith(type.ToLower()))
                {
                    webr.Transfer(url, groupId, seq, type);
                }
                else
                {

                    webr.Transfer(new Uri(String.Format("{0}?x-oss-process=image/format,{1}", media_id, type)), groupId, seq, type);
                }


            }
            else
            {
                /*
                 * 微信上传
                 * */
            }


            this.Context.Send(new WebMeta().Put("type", "image").Put("id", groupId), true);


        }

    }

}