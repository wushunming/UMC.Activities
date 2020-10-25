
using System;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignPictureActivity : WebActivity
    {

        void UploadImage(Guid group_id, int seq, string UserHostAddress, Guid? userid)
        {
            var entity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Picture>();

            entity.Where.And().Equal(new UMC.Data.Entities.Picture { Seq = seq, group_id = group_id });
            if (entity.Update(new UMC.Data.Entities.Picture
            {
                Location = UserHostAddress,
                UploadDate = DateTime.Now,
                user_id = userid
            }) == 0)
            {
                var photo = new UMC.Data.Entities.Picture
                {
                    Location = UserHostAddress,
                    group_id = group_id,
                    Seq = seq,
                    user_id = userid,
                    UploadDate = DateTime.Now
                };
                entity.Insert(photo);
            }

        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;
            var groupId = UMC.Data.Utility.Guid(this.AsyncDialog("id", d =>
            {
                this.Prompt("请传入参数");
                return this.DialogValue(user.Id.ToString());
            }), true) ?? Guid.Empty;

            var Seq = this.AsyncDialog("seq", g =>
            {
                if (request.SendValues != null)
                {

                    return this.DialogValue(request.SendValues["Seq"] ?? "0");
                }
                else
                {
                    return this.DialogValue("0");
                }
            });
            WebResource oosr = WebResource.Instance();//as OssResource;
            var media_id = UMC.Web.UIDialog.AsyncDialog("media_id", g =>
            {
                if (request.IsApp)
                {
                    var f = Web.UIDialog.CreateDialog("File");
                    f.Config.Put("Submit", new UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(g, "Value"))
                    {
                        Command = request.Command,
                        Model = request.Model
                    });
                    return f;

                }
                else
                {

                    var webr = UMC.Data.WebResource.Instance();
                    var from = new Web.UIFormDialog() { Title = "图片上传" };

                    from.AddFile("选择图片", "media_id", webr.ImageResolve(groupId, "1", 4));

                    from.Submit("确认上传", request, "image");
                    return from;
                }
            });

            var pictureEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Picture>();
            pictureEntity.Order.Asc(new Data.Entities.Picture { Seq = 0 });
            pictureEntity.Where.Reset().And().Equal(new Data.Entities.Picture { group_id = groupId });
            if (String.Equals(media_id, "none"))
            {
                var seq = UMC.Data.Utility.Parse(Seq, 0);
                if (request.IsApp == false)
                    this.AsyncDialog("Confirm", s =>
                    {

                        return new Web.UIConfirmDialog(String.Format("确认删除此组第{0}张图片吗", seq)) { Title = "删除提示" };

                    });

                if (seq == 1)
                {
                    pictureEntity.Where.And().Greater(new Data.Entities.Picture { Seq = 1 });
                    var seqs = pictureEntity.Single();
                    if (seqs != null)
                    {
                        UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Picture>()
                            .Where.And().Equal(new Data.Entities.Picture { Seq = 1, group_id = groupId })
                            .Entities.Update(new Data.Entities.Picture { UploadDate = DateTime.Now });
                        pictureEntity.Where.And().Equal(new UMC.Data.Entities.Picture { Seq = seqs.Seq });

                        oosr.Transfer(new Uri(oosr.ResolveUrl(seqs.group_id.Value, seqs.Seq, 0)), groupId, seq);
                    }
                    else
                    {
                        pictureEntity.Where.Reset().And().Equal(new UMC.Data.Entities.Picture
                        {
                            Seq = 1,
                            group_id = groupId
                        });

                    }
                    pictureEntity.Delete();
                }
                else
                {
                    pictureEntity.Where.And().Equal(new UMC.Data.Entities.Picture { Seq = seq });
                    pictureEntity.Delete();
                }


            }
            else
            {
                var type = this.AsyncDialog("type", g => this.DialogValue("jpg"));
                var seq = UMC.Data.Utility.Parse(Seq, -1);
                if (media_id.StartsWith("http://") || media_id.StartsWith("https://"))
                {
                    var url = new Uri(media_id);
                    if (url.Host.StartsWith("oss."))
                    {
                        if (seq > -1)
                        {
                            if (seq < 1)
                            {
                                seq = (pictureEntity.Max(new Data.Entities.Picture { Seq = 0 }).Seq ?? 0) + 1;
                            }

                            UploadImage(groupId, seq, request.UserHostAddress, user.Id);
                        }
                        if (url.AbsolutePath.EndsWith(type, StringComparison.CurrentCultureIgnoreCase))
                        {
                            oosr.Transfer(url, groupId, seq, type);
                        }
                        else
                        {

                            oosr.Transfer(new Uri(String.Format("{0}?x-oss-process=image/format,{1}", media_id, type)), groupId, seq, type);
                        }
                    }
                    else
                    {

                        if (seq < 1)
                        {
                            seq = (pictureEntity.Max(new Data.Entities.Picture { Seq = 0 }).Seq ?? 0) + 1;
                        }
                        UploadImage(groupId, seq, request.UserHostAddress, user.Id);
                        oosr.Transfer(new Uri(media_id), groupId, seq);
                    }

                }
            }

            this.Context.Send(new WebMeta().Put("type", "image").Put("id", groupId.ToString()), true);


        }


    }

}