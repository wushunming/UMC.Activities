

using System.Collections;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    public class RoleDialog : UIGridDialog
    {

        protected override Hashtable GetHeader()
        {
            IsAsyncData = true;

            Header header = new Header("Id", 25);
            header.AddField("Rolename", "角色名");
            return header.GetHeader();
        }

        protected override Hashtable GetData(IDictionary paramsKey)
        {
            var roleIObjectEntity = UMC.Data.Database.Instance().ObjectEntity<Role>();

            var hash = new Hashtable();
            hash["data"] = roleIObjectEntity.Query();
            hash["totla"] = roleIObjectEntity.Count();
            return hash;
        }
    }
}
