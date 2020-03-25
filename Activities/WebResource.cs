using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    public class WebResource : Data.WebResource
    {

        public override string ResolveUrl(string path)
        {
            //return base.ResolveUrl(path);

            String vUrl = path;
            if (path.StartsWith("~/"))
            {
                vUrl = path.Substring(1);
            }
            else if (path.StartsWith("~"))
            {
                vUrl = "/" + path.Substring(1);
            }
            String src = this.Provider["src"];
            if (String.IsNullOrEmpty(src))
            {

                String vpath = this.Provider["authkey"];

                if (String.IsNullOrEmpty(vpath) == false)
                {
                    String code = Utility.ParseEncode(Utility.Guid(vpath).GetHashCode(), 36);
                    vpath = code;// + "/";
                }

                return String.Format("http://image.365lu.cn/{0}{1}", vpath, vUrl);


            }
            return src + vUrl;
        }
        public override void Transfer(Uri uri, Guid guid, int seq, string type)
        {


            String vpath = this.Provider["authkey"];
            if (String.IsNullOrEmpty(vpath) == false)
            {
                String code = Utility.ParseEncode(Utility.Guid(vpath).GetHashCode(), 36);
                vpath = code + "/";


                String key = String.Format("{0}images/{1}/{2}/{3}.{4}", vpath, guid, seq, 0, type.ToLower());


                String sts = String.Format("https://ali.365lu.cn/OSS/Transfer/%s", this.Provider["authkey"]);


                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                var res = httpClient.PostAsync(sts, new System.Net.Http.StringContent(JSON.Serialize(new WebMeta().Put("src", uri.AbsoluteUri, "key", key)))).Result;// ().Result;



            }

        }
        public override void Transfer(Uri uri, Guid guid, int seq)
        {
            Transfer(uri, guid, seq, "jpg");
        }

    }
}