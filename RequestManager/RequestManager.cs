using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RequestManager
{

    public class RequestResult
    {
        public bool IsOK { get; set; }
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public HttpStatusCode? HttpStatusCode { get; set; }
    }

    public static class RequestType
    {
        public static readonly string JSON = "application/json";
        public static readonly string XML = "text/xml";
    }

    public class RequestModel
    {
        public string EndpointUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Method { get; set; } = "POST";
        public string Type { get; set; }
        public string Token { get; set; }
        public string Accept { get; set; }
        public byte[] ByteData { get; set; }
        public string PostData { get; set; }

    }

    public class RequestManager
    {
        public string ServiceURL { get; set; }
        public string UserAgent = string.Empty;
        public List<string> HeaderParameters = null;

        public RequestManager(string serviceUrl, string userAgent = "", List<string> headerParameters = null)
        {
            ServiceURL = serviceUrl;

            if (!string.IsNullOrEmpty(userAgent))
                UserAgent = userAgent;

            if (headerParameters != null && headerParameters.Any())
                HeaderParameters = headerParameters;
        }
        public RequestManager()
        {

        }

        public RequestResult SendRequest(string endPointUrl, string postData, string userName = "", string password = "", string method = "POST", string type = "", string token = "")
        {
            if (string.IsNullOrEmpty(type))
                type = RequestType.JSON;

            RequestResult result = new RequestResult();

            try
            {
                Uri uri = new Uri(ServiceURL + "/" + endPointUrl, UriKind.Absolute);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = type;
                httpWebRequest.Method = method;

                if (!string.IsNullOrEmpty(UserAgent))
                    httpWebRequest.UserAgent = UserAgent;

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                {
                    string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(userName + ":" + password));
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
                }
                if (!string.IsNullOrEmpty(token))
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "bearer " + token);

                if (HeaderParameters != null && HeaderParameters.Any())
                {
                    var referer = HeaderParameters.FirstOrDefault(x => x.StartsWith("Referer"));
                    if (!string.IsNullOrEmpty(referer))
                    {
                        var splitReferer = referer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        string newReferer = splitReferer[1] + ":" + splitReferer[2];

                        httpWebRequest.Referer = newReferer;
                        HeaderParameters.RemoveAll(x => x == referer);
                    }

                    foreach (string item in HeaderParameters)
                    {
                        httpWebRequest.Headers.Add(item);
                    }
                }

                if (!string.IsNullOrEmpty(postData))
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write(postData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                using (var response = httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        result.IsOK = true;
                        result.Response = streamReader.ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                result.Response = string.Empty;
                result.IsOK = false;
                result.ErrorMessage = FullMessage(wex);
                result.HttpStatusCode = (wex.Response as HttpWebResponse)?.StatusCode;

                if (wex.Response != null)
                {
                    var exResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    if (!string.IsNullOrEmpty(exResponse))
                        result.Response = exResponse;
                }
            }
            catch (Exception ex)
            {
                //TODO: Loglama sistemine alınıcak
                result.IsOK = false;
                result.Response = string.Empty;
                result.ErrorMessage = FullMessage(ex);
            }

            return result;
        }

        public RequestResult SendRequest(RequestModel requestModel)
        {

            if (string.IsNullOrEmpty(requestModel.Type))
                requestModel.Type = RequestType.JSON;

            RequestResult result = new RequestResult();

            try
            {
                Uri uri = new Uri(ServiceURL + "/" + requestModel.EndpointUrl, UriKind.Absolute);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = requestModel.Type;
                httpWebRequest.Method = requestModel.Method;

                if (string.IsNullOrEmpty(requestModel.Accept))
                    httpWebRequest.Accept = requestModel.Accept;

                if (!string.IsNullOrEmpty(UserAgent))
                    httpWebRequest.UserAgent = UserAgent;

                if (!string.IsNullOrEmpty(requestModel.UserName) && !string.IsNullOrEmpty(requestModel.Password))
                {
                    string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(requestModel.UserName + ":" + requestModel.UserName));
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
                }
                if (!string.IsNullOrEmpty(requestModel.Token))
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "bearer " + requestModel.Token);

                if (HeaderParameters != null && HeaderParameters.Any())
                {
                    foreach (string item in HeaderParameters)
                    {
                        httpWebRequest.Headers.Add(item);
                    }
                }

                if (requestModel.ByteData != null)
                {
                    using (Stream dataStream = httpWebRequest.GetRequestStream())
                    {
                        dataStream.Write(requestModel.ByteData, 0, requestModel.ByteData.Length);
                        dataStream.Flush();
                        dataStream.Close();
                    }
                }

                using (var response = httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        result.IsOK = true;
                        result.Response = streamReader.ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                result.Response = string.Empty;
                result.IsOK = false;
                result.ErrorMessage = FullMessage(wex);

                if (wex.Response != null)
                {
                    var exResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    if (!string.IsNullOrEmpty(exResponse))
                        result.Response = exResponse;
                }
            }
            catch (Exception ex)
            {
                result.IsOK = false;
                result.Response = string.Empty;
                result.ErrorMessage = FullMessage(ex);
            }

            return result;


        }

        private string FullMessage(Exception exp)
        {
            return "Message: " + exp.Message + " Inner Exception: " + ((exp.InnerException != null && !string.IsNullOrEmpty(exp.InnerException.Message)) ? exp.InnerException.Message : "NULL");
        }
    }
}
