using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Common
{
    public class ResponseStatus
    {
        private class StatusInfo
        {
            public string requestURL { get; set; }
            public int statusCode { get; set; }
            public string statusString { get; set; }
            public string subStatusCode { get; set; }
            public int errorCode { get; set; }
            public string errorMsg { get; set; }
        }

        public static string AnalysisResponseStatus(string strResponseStatus)
        {
            string strAnalysisData = string.Empty;

            if (strResponseStatus.Contains("</ResponseStatus>"))
            {
                strAnalysisData = AnalysisXml(strResponseStatus);
            }
            else if (strResponseStatus.Contains("\"statusString\""))
            {
                strAnalysisData = AnalysisJson(strResponseStatus);
            }
            else
            {
                strAnalysisData = strResponseStatus;
            }

            return strAnalysisData;
        }

        private static string AnalysisXml(string strXml)
        {
            string strAnalysisData = string.Empty;
            try
            {
                StatusInfo cStatusInfo = new StatusInfo();

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(strXml);

                XmlNode root = xml.DocumentElement;
                if ("ResponseStatus" == root.Name)
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if ("requestURL" == node.Name)
                        {
                            cStatusInfo.requestURL = node.InnerText;
                        }
                        else if ("statusCode" == node.Name)
                        {
                            cStatusInfo.statusCode = int.Parse(node.InnerText);
                        }
                        else if ("statusString" == node.Name)
                        {
                            cStatusInfo.statusString = node.InnerText;
                        }
                        else if ("subStatusCode" == node.Name)
                        {
                            cStatusInfo.subStatusCode = node.InnerText;
                        }
                    }

                    strAnalysisData = "statusString[" + cStatusInfo.statusString + "]" + "subStatusCode[" + cStatusInfo.subStatusCode + "]";
                }

                return strAnalysisData;
            }
            catch (Exception)
            {
                strAnalysisData = strXml;
                return strAnalysisData;
            }
        }

        private static string AnalysisJson(string strJson)
        {
            string strAnalysisData = string.Empty;
            try
            {
                StatusInfo cStatusInfo = new StatusInfo();
                cStatusInfo = JsonConvert.DeserializeObject<StatusInfo>(strJson);

                strAnalysisData = "statusString[" + cStatusInfo.statusString + "]" + "subStatusCode[" + cStatusInfo.subStatusCode + "]";

                return strAnalysisData;
            }
            catch (Exception)
            {
                strAnalysisData = strJson;
                return strAnalysisData;
            }
        }
    }
}
