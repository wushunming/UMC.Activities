
using System;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignConfigActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            String group = request.Command;

            var entity = Database.Instance().ObjectEntity<Design_Config>();
            entity.Order.Asc(new Design_Config() { Sequence = 0 });


            Guid? vid = UMC.Data.Utility.Guid(this.AsyncDialog("Id", s =>
            {
                entity.Where.And().Equal(new Design_Config() { GroupBy = (group) });

                UIGridDialog rdoDig = UIGridDialog.Create(new UIGridDialog.Header("Id", 0)
                                .PutField("Name", "标题").PutField("Value", "代码")
                        , entity.Query());
                rdoDig.Menu("新建配置", request.Model, request.Command, "News");
                rdoDig.RefreshEvent = "Settings";
                rdoDig.IsPage = (true);// = true;
                rdoDig.Title = ("数据配置");


                return rdoDig;
            }));// ??Guid.Empty;

            WebMeta configs = this.AsyncDialog(s =>
            {
                UIFormDialog fm = new UIFormDialog();
                if (vid == null)
                {
                    fm.Title = ("新增配置值");
                }
                else
                {
                    fm.Title = ("修改配置值");
                }
                entity.Where.And().Equal(new Design_Config() { Id = (vid) });

                Design_Config con = null;
                if (vid != null)
                {
                    con = entity.Single();
                }
                if (con == null)
                {
                    entity.Where.Reset().And().Equal(new Design_Config() { GroupBy = (group) });
                    con = entity.Max(new Design_Config() { Sequence = (0) });
                }

                fm.AddText("配置名称", "Name", con.Name);
                fm.AddText("配置标题", "Value", con.Value);
                fm.AddNumber("显示顺序", "Sequence", con.Sequence);
                return fm;
            }, "Config");
            Design_Config cv = new Design_Config();
            UMC.Data.Reflection.SetProperty(cv, configs.GetDictionary());
            if (vid.HasValue == false)
            {
                cv.GroupBy = group;
                cv.Id = Guid.NewGuid();///.randomUUID();
                entity.Insert(cv);
            }
            else
            {
                cv.Id = vid;
                entity.Where.Reset().And().Equal(new Design_Config() { Id = (vid) });

                if (cv.Sequence == -1)
                {
                    entity.Delete();
                }
                else
                {
                    entity.Update(cv);
                }
            }
            this.Context.Send("Settings", true);
        }
    }

}