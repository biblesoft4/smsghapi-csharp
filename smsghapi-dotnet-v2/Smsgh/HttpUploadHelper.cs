// http://aspnetupload.com
// Copyright � 2009 Krystalware, Inc.
//
// This work is licensed under a Creative Commons Attribution-Share Alike 3.0 United States License
// http://creativecommons.org/licenses/by-sa/3.0/us/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace smsghapi_dotnet_v2.Smsgh
{
    public class HttpUploadHelper
    {
        private HttpUploadHelper() {}

        public static string Upload(string url, UploadFile[] files, NameValueCollection form)
        {
            HttpWebResponse resp = Upload((HttpWebRequest) WebRequest.Create(url), files, form);

            using (Stream s = resp.GetResponseStream())
                if (s != null)
                    using (var sr = new StreamReader(s)) {
                        return sr.ReadToEnd();
                    }
            return null;
        }

        public static HttpWebResponse Upload(HttpWebRequest req, UploadFile[] files, NameValueCollection form)
        {
            var mimeParts = new List<MimePart>();

            try {
                foreach (string key in form.AllKeys) {
                    var part = new StringMimePart();
                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.StringData = form[key];
                    mimeParts.Add(part);
                }

                int nameIndex = 0;
                foreach (UploadFile file in files) {
                    var part = new StreamMimePart();
                    if (string.IsNullOrEmpty(file.FieldName))
                        file.FieldName = "file" + nameIndex++;

                    part.Headers["Content-Disposition"] = "form-data; name=\"" + file.FieldName + "\"; filename=\"" + file.FileName + "\"";
                    part.Headers["Content-Type"] = file.ContentType;
                    part.SetStream(file.Data);
                    mimeParts.Add(part);
                }

                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                req.ContentType = "multipart/form-data; boundary=" + boundary;
                //req.Method = "POST";
                byte[] footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
                long contentLength = mimeParts.Sum(part => part.GenerateHeaderFooterData(boundary));

                //foreach (MimePart part in mimeParts)
                //{
                //    contentLength += part.GenerateHeaderFooterData(boundary);
                //}
                req.ContentLength = contentLength + footer.Length;

                var buffer = new byte[8192];
                byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");

                using (Stream s = req.GetRequestStream()) {
                    foreach (MimePart part in mimeParts) {
                        s.Write(part.Header, 0, part.Header.Length);
                        int read;
                        while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                            s.Write(buffer, 0, read);
                        part.Data.Dispose();
                        s.Write(afterFile, 0, afterFile.Length);
                    }
                    s.Write(footer, 0, footer.Length);
                }
                return (HttpWebResponse) req.GetResponse();
            }
            catch {
                foreach (MimePart part in mimeParts)
                    if (part.Data != null)
                        part.Data.Dispose();
                throw;
            }
        }
    }
}