
using System;
using UMC.Web;

namespace UMC.Activities
{
    class AccountLinkActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var url = this.AsyncDialog("Url", g =>
            {
                this.Prompt("缺少参数");
                return new UITextDialog();
            });
            var Caption = this.AsyncDialog("Caption", url).Trim();
            var groupBy = this.AsyncDialog("GroupBy", "UMC");
            var Id = UMC.Data.Utility.Guid(this.AsyncDialog("Id", url), true);

            var user = UMC.Security.Identity.Current;
            var lineEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Link>();
            lineEntity.Where.And().Equal(new UMC.Data.Entities.Link { Id = Id });
            var type = this.AsyncDialog("Type", "Save");
            switch (type)
            {
                case "Best":
                    {
                        var IsBest = false;
                        var entity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Proposal>()
                                 .Where.And().Equal(new UMC.Data.Entities.Proposal { user_id = user.Id, ref_id = Id }).Entities;

                        entity.IFF(e =>
                        {
                            if (e.Delete() > 0)
                            {
                                lineEntity.Update("{0}+{1}", new UMC.Data.Entities.Link { Favs = -1 });
                                return false;
                            }
                            return true;
                        }, e =>
                        {
                            IsBest = true;
                            e.Insert(new UMC.Data.Entities.Proposal
                            {
                                ref_id = Id,
                                user_id = user.Id,
                                Poster = user.Alias,
                                CreationDate = DateTime.Now,
                                Type = UMC.Data.Entities.ProposalType.Effective
                            });
                            lineEntity.IFF(e1 => e1.Update("{0}+{1}", new UMC.Data.Entities.Link { Favs = 1 }) == 0, e1 =>
                            e1.Insert(new UMC.Data.Entities.Link
                            {
                                Id = Id,
                                Favs = 1,
                                Times = 1,
                                Caption = Caption,
                                ModifiedTime = DateTime.Now,
                                IsMenu = false,
                                CreationTime = DateTime.Now,
                                Url = url,
                                GroupBy = groupBy
                            }));
                        });
                        this.Context.Send("Link", new WebMeta().Put("link", lineEntity.Single()).Put("Best", IsBest), true);
                    }
                    break;
                case "Help":
                    {
                        var entity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Subject>()
                                .Where.And().Equal(new UMC.Data.Entities.Subject { Id = Id }).Entities;
                        if (entity.Count() > 0)
                        {
                            this.Context.Send("Help", new WebMeta().Put("Id", Id), true);
                        }
                        else if (request.IsMaster)
                        {
                            //var line2 = lineEntity.Single();
                            //if (line2 == null)
                            //{
                            this.Prompt("请先收录，再来编写帮助文档");
                            //}
                            //this.AsyncDialog("Config", g => new Web.UIConfirmDialog("您是管理员，需要生成此项的帮助文档吗？"));
                            //var cateEntity = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Category>()
                            //       .Where.And().Equal(new Data.Entities.Category { Caption = "帮助文档" }).Entities;
                            //var help = cateEntity.Single();
                            //if (help == null)
                            //{
                            //    help = new Data.Entities.Category { Id = Guid.NewGuid(), Attentions = 0, Count = 0, Caption = "帮助文档", Visible = Data.Entities.Visibility.Hidden };
                            //    cateEntity.Insert(help);
                            //}

                            //entity.Insert(new Data.Entities.Subject { Id = Id, Title = line2.Caption, Favs = 0, category_id = help.Id });
                            //this.Context.Send("Subject", new WebMeta().Put("Id", Id).Put("Model", "Editer"), true);
                        }
                        else
                        {
                            this.Prompt("此功能未编写帮助文档");
                        }


                    }
                    break;
            }

            var line = lineEntity.Single();
            if (line != null)
            {
                lineEntity.Update("{0}+{1}", new UMC.Data.Entities.Link() { Times = 1 }, new UMC.Data.Entities.Link { ModifiedTime = DateTime.Now });
                line.Times += 1;
            }
            else
            {
                if (String.IsNullOrEmpty(request.SendValue) == false)
                {
                    return;
                }
                line = new UMC.Data.Entities.Link
                {
                    Id = Id,
                    Favs = 0,
                    Times = 1,
                    Caption = Caption,
                    ModifiedTime = DateTime.Now,
                    IsMenu = false,
                    CreationTime = DateTime.Now,
                    Url = url,
                    GroupBy = groupBy
                };
                lineEntity.Insert(line);

            }
            UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.LinkAccess>()
                .Where.And().Equal(new UMC.Data.Entities.LinkAccess
                {
                    link_id = line.Id,
                    Username = UMC.Data.Utility.GetUsername()
                })
                .Entities.IFF(e => e.Update("{0}+{1}", new UMC.Data.Entities.LinkAccess { Times = 1 }
                , new UMC.Data.Entities.LinkAccess { LastAccessTime = DateTime.Now }) == 0,
                e => e.Insert(new UMC.Data.Entities.LinkAccess
                {
                    Times = 1,
                    LastAccessTime = DateTime.Now,
                    link_id = line.Id,
                    Username = UMC.Data.Utility.GetUsername()
                }));

            var best = UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Proposal>()
                            .Where.And().Equal(new UMC.Data.Entities.Proposal { user_id = user.Id, ref_id = Id }).Entities.Count();

            this.Context.Send("Link", new WebMeta().Put("link", line).Put("Best", best), true);

        }
    }
}