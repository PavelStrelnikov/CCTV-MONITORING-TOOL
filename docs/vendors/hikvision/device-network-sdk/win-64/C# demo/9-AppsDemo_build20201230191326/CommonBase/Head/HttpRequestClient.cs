using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;

namespace Common
{
  /// <summary>
    /// description：http post Requesting client
  /// last-modified-date：2012-02-28
  /// </summary>
  public class HttpRequestClient
  {
    #region //field
    private ArrayList bytesArray;
    private Encoding encoding = Encoding.UTF8;
    private string boundary = String.Empty;
    #endregion

    #region //constructor
    public HttpRequestClient()
    {
      bytesArray = new ArrayList();
      string flag = DateTime.Now.Ticks.ToString("x");
      boundary = "---------------------------"  + flag;
    }
    #endregion

    #region //function
    /// <summary>
    /// Merge request data
    /// </summary>
    /// <returns></returns>
    private byte[] MergeContent()
    {
        int length = 0;
        int readLength = 0;
        string endBoundary = "\r\n--" + boundary + "--\r\n";
        byte[] endBoundaryBytes = encoding.GetBytes(endBoundary);
        bytesArray.Add(endBoundaryBytes);
        foreach (byte[] b in bytesArray)
        {
            length += b.Length;
        }
        byte[] bytes = new byte[length];
        foreach (byte[] b in bytesArray)
        {
            b.CopyTo(bytes, readLength);
            readLength += b.Length;
        }
        return bytes;
    }

    /// <summary>
    /// Upload
    /// </summary>
    /// <param name="requestUrl">Request url</param>
    /// <param name="responseText">Response text</param>
    /// <returns></returns>
    public bool Upload(ICredentials Credentials, String requestUrl, out String responseText)
    {
        WebClient webClient = new WebClient();
        webClient.Credentials = Credentials;
        webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
        byte[] responseBytes;
        byte[] bytes = MergeContent();
        try
        {
            responseBytes = webClient.UploadData(requestUrl, bytes);
            responseText = System.Text.Encoding.UTF8.GetString(responseBytes);
            return true;
        }
        catch (WebException ex)
        {

            WebResponse wr = ex.Response;
            if (wr != null)
            {
                Stream responseStream = ex.Response.GetResponseStream();
                responseBytes = new byte[ex.Response.ContentLength];
                responseStream.Read(responseBytes, 0, responseBytes.Length);
            }
            else
            {
                string str = "Unknow Error";
                responseBytes = System.Text.Encoding.UTF8.GetBytes(str);
            }
        }
        responseText = System.Text.Encoding.UTF8.GetString(responseBytes);
        return false;
    }

    /// <summary>
    /// Set the form data fields
    /// </summary>
    /// <param name="fieldName">fieldName</param>
    /// <param name="fieldValue">fieldValue</param>
    /// <returns></returns>
    public void SetFieldValue(string fieldName, string fieldValue, string contentType)
    {
        string httpRow = "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"" + fieldName + "\";\r\n";
        httpRow += "Content-Type: " + contentType + "\r\n";
        httpRow += "Content-Length: " + fieldValue.Length.ToString() + "\r\n\r\n";
        httpRow += fieldValue + "\r\n";
        bytesArray.Add(encoding.GetBytes(httpRow));
    }

    /// <summary>
    /// Set the form data fields
    /// </summary>
    /// <param name="fieldName">fieldName</param>
    /// <param name="filename">filename</param>
    /// <param name="contentType">contentType</param>
    /// <param name="fileBytes">fileBytes</param>
    /// <returns></returns>
    public void SetFieldValue(string fieldName, string filename, string contentType, byte[] fileBytes)
    {
        string httpRow = "--" + boundary + "\r\n"
                    + "Content-Disposition: form-data; name=\"" + fieldName + "\"; filename=\"" + filename + "\";\r\n"
                    + "Content-Type: " + contentType + "\r\n"
                    + "Content-Length: " + fileBytes.Length.ToString() + "\r\n\r\n";

        byte[] headerBytes = encoding.GetBytes(httpRow);
        byte[] fileDataBytes = new byte[headerBytes.Length + fileBytes.Length];

        headerBytes.CopyTo(fileDataBytes, 0);
        fileBytes.CopyTo(fileDataBytes, headerBytes.Length);
        bytesArray.Add(fileDataBytes);
    }
    #endregion
  }
}
