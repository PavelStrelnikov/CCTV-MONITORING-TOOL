using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public class WebClientEx : WebClient
    {
        public WebClientEx() : base()
        {
            Timeout = 5000; //默认超时时间2秒
        }
        public int Timeout { get; set; }
        public string ErrorMessage { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }

        //responseString在设备返回正确的时候返回设备信息
        //在设备返回错误的时候返回错误信息
        private bool DoResquest(string method, string url, string requestString, out string responseString)
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            responseString = "";
            ErrorMessage = "";
            try
            {
                byte[] byRequest;
                //判断字符串中是否有中文
                Regex r = new Regex(@"[\u4e00-\u9fa5]+");
                Match mc = r.Match(requestString);
                if(mc.Length != 0)
                {
                    byRequest = System.Text.Encoding.Default.GetBytes(requestString);
                }
                else
                {
                    byRequest = Encoding.UTF8.GetBytes(requestString);     
                }
                         
                byte[] byResponse = this.UploadData(url, method, byRequest);
                responseString = Encoding.UTF8.GetString(byResponse);
            }
            catch(WebException ex)
            {
                
                Stream resp = ex.Response.GetResponseStream();
                byte[] byResponse = new byte[ex.Response.ContentLength];
                resp.Read(byResponse, 0, byResponse.Length);
                responseString = Encoding.UTF8.GetString(byResponse);
                this.ErrorMessage = ex.Message;
                return false;
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
                return false;
            }
            return true;
        }

        public bool DoGet(string url, out string responseString)
        {
            responseString = "";
            try
            {
                byte[] responseData = DownloadData(url);

                if (responseData != null)
                {
                    responseString = Encoding.UTF8.GetString(responseData);
                    return true;
                }
            }
            catch (WebException ex)
            {

                Stream resp = ex.Response.GetResponseStream();
                byte[] byResponse = new byte[ex.Response.ContentLength];
                resp.Read(byResponse, 0, byResponse.Length);
                responseString = Encoding.UTF8.GetString(byResponse);
                this.ErrorMessage = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
            return true;
        }

        public bool DoPut(string url, string requestString, out string responseString)
        {
            return this.DoResquest("PUT", url, requestString, out responseString);
        }

        public bool DoPost(string url, string requestString, out string responseString)
        {
            return this.DoResquest("POST", url, requestString, out responseString);
        }

        public bool DoDelete(string url, out string responseString)
        {
            return this.DoResquest("DELETE", url, "", out responseString);
        }

        public bool DoCondDelete(string url, string requestString, out string responseString)
        {
            return this.DoResquest("DELETE", url, requestString, out responseString);
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
