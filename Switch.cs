using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Switch
{
    public class SwitchAPI
    {
        public string APIEndpoint = "https://tr02.switchapi.com";
        public SwitchAuth ReadAccess { get; set; }
        public SwitchAuth WriteAccess { get; set; }
        public SwitchAuth ModifyAccess { get; set; }
        public SwitchAuth DeleteAccess { get; set; }

        public void Initialize(DateTime ExpireTime)
        {
            if (ReadAccess != null)
            {
                ReadAccess.AccessToken = GetToken(ReadAccess, ExpireTime);
            }
            if (WriteAccess != null)
            {
                WriteAccess.AccessToken = GetToken(WriteAccess, ExpireTime);
            }
            if (ModifyAccess != null)
            {
                ModifyAccess.AccessToken = GetToken(ModifyAccess, ExpireTime);
            }
            if (DeleteAccess != null)
            {
                DeleteAccess.AccessToken = GetToken(DeleteAccess, ExpireTime);
            }
        }

        public string GetToken(SwitchAuth auth, DateTime ExpireTime)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", auth.APIKey);
            wc.Headers.Add("Signature", md5(string.Format("{0}{1}", auth.APISecret, GetUnixTimestampMillis(ExpireTime))));
            wc.Headers.Add("Expire", GetUnixTimestampMillis(ExpireTime).ToString());
            wc.Encoding = Encoding.UTF8;

            string response = wc.DownloadString(string.Format("{0}/Token", APIEndpoint));

            try
            {
                dynamic tokenResponse = JsonConvert.DeserializeObject(response);
                return tokenResponse.AccessToken.Value;
            }
            catch (Exception ex)
            {
                // TODO : Exception handle
                return string.Empty;
            }
        }

        public string List(string ListName, string Query)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", ReadAccess.APIKey);
            wc.Headers.Add("AccessToken", ReadAccess.AccessToken);
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;

            return wc.UploadString(string.Format("{0}/List", APIEndpoint), Query);
        }

        public string Add(string ListName, string JsonData)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", WriteAccess.APIKey);
            wc.Headers.Add("AccessToken", WriteAccess.AccessToken);
            wc.Headers.Add("List", ListName);
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;

            return wc.UploadString(string.Format("{0}/Add", APIEndpoint), JsonData);
        }

        public string Update(string ListName, string ListItemId, string JsonData)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", ModifyAccess.APIKey);
            wc.Headers.Add("AccessToken", ModifyAccess.AccessToken);
            wc.Headers.Add("List", ListName);
            wc.Headers.Add("ListItemId", ListItemId);
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;

            return wc.UploadString(string.Format("{0}/Set", APIEndpoint), JsonData);
        }

        public string Delete(string ListName, string ListItemId)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", DeleteAccess.APIKey);
            wc.Headers.Add("AccessToken", DeleteAccess.AccessToken);
            wc.Headers.Add("List", ListName);
            wc.Headers.Add("ListItemId", ListItemId);
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;

            return wc.UploadString(string.Format("{0}/Set", APIEndpoint), "DELETE", string.Empty);
        }

        public string SendGridSend(SendGridMail EMail)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("APIKey", WriteAccess.APIKey);
            wc.Headers.Add("AccessToken", WriteAccess.AccessToken);
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;

            return wc.UploadString(string.Format("{0}/SendGrid/Send", APIEndpoint), "POST", JsonConvert.SerializeObject(EMail));
        }

        #region Switch Helpers
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetUnixTimestampMillis(DateTime dateTime)
        {
            return (long)(dateTime - UnixEpoch).TotalMilliseconds;
        }

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static string md5(string plainText)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] btr = Encoding.UTF8.GetBytes(plainText);
            btr = md5.ComputeHash(btr);

            StringBuilder sb = new StringBuilder();

            foreach (byte ba in btr)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }

            return sb.ToString();
        }

        public class SwitchAuth
        {
            public string APIKey { get; set; }
            public string APISecret { get; set; }
            public string AccessToken { get; set; }
        }
        #endregion

        #region SendGrid Mail Objects
        public class To
        {
            public string email { get; set; }
        }

        public class Personalization
        {
            public List<To> to { get; set; }
            public string subject { get; set; }
        }

        public class From
        {
            public string email { get; set; }
            public string name { get; set; }
        }

        public class Content
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public class SendGridMail
        {
            public List<Personalization> personalizations { get; set; }
            public From from { get; set; }
            public List<Content> content { get; set; }
        }
        #endregion
    }
}