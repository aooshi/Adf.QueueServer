using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace Adf.QueueServer
{
    class HttpHandler
    {
        QueueManager queueManager = null;
        Adf.Service.ServiceContext serviceContext = null;
        Adf.HttpServer httpServer = null;

        public static string hostname;
        public static Version version;

        static HttpHandler()
        {
            hostname = Dns.GetHostName();
            version = typeof(HttpHandler).Assembly.GetName().Version;
        }

        public HttpHandler()
        {
            this.httpServer = Program.ServiceContext.HttpServer;
            this.queueManager = Program.QueueManager;
            this.serviceContext = Program.ServiceContext;
        }

        public HttpStatusCode Request(HttpServerContext httpContext)
        {
            var statusCode = HttpStatusCode.NotFound;
            var path = httpContext.Path;
            // if bin or json
            var bin = path.EndsWith(".bin");
            if (bin == true)
            {
                path = path.Substring(0, path.Length - 4);
            }
            else if (path.EndsWith(".json"))
            {
                path = path.Substring(0, path.Length - 5);
            }
            //
            httpContext.ResponseHeader["Via"] = HttpHandler.hostname;
            //
            if ("/queue/rpush" == path)
            {
                statusCode = this.Push(Action.RPUSH, httpContext, bin);
            }
            else if ("/queue/lpush" == path)
            {
                statusCode = this.Push(Action.LPUSH, httpContext, bin);
            }
            else if ("/queue/pull" == path)
            {
                statusCode = this.Pull(httpContext, bin);
            }
            else if ("/queue/delete" == path)
            {
                statusCode = this.General(new Action(), Action.DELETE, httpContext, bin);
            }
            else if ("/queue/lcancel" == path)
            {
                statusCode = this.General(new Action(), Action.LCANCEL, httpContext, bin);
            }
            else if ("/queue/rcancel" == path)
            {
                statusCode = this.General(new Action(), Action.RCANCEL, httpContext, bin);
            }
            else if ("/queue/count" == path)
            {
                statusCode = this.General(new CountAction(), Action.COUNT, httpContext, bin);
            }
            else if ("/queue/clear" == path)
            {
                statusCode = this.General(new ClearAction(), Action.CLEAR, httpContext, bin);
            }
            else if ("/queue/createqueue" == path)
            {
                statusCode = this.General(new Action(), Action.CREATEQUEUE, httpContext, bin);
            }
            else if ("/queue/deletequeue" == path)
            {
                statusCode = this.General(new Action(), Action.DELETEQUEUE, httpContext, bin);
            }
            else if ("/ajax.js" == path)
            {
                statusCode = this.OutputScript(httpContext, "ajax.js");
            }
            else if ("/control.js" == path)
            {
                statusCode = this.OutputScript(httpContext, "control.js");
            }
            else
            {
                statusCode = this.Home(httpContext);
            }

            return statusCode;
        }

        private HttpStatusCode OutputScript(HttpServerContext httpContext, string name)
        {
            var root = System.IO.Path.GetDirectoryName(typeof(HttpHandler).Assembly.Location);
            var file = System.IO.Path.Combine(root, name);

            HttpStatusCode statusCode = HttpStatusCode.OK;

            try
            {
                httpContext.ResponseHeader["Content-Type"] = "text/javascript";
                httpContext.ContentBuffer = System.IO.File.ReadAllBytes(file);
            }
            catch (System.IO.FileNotFoundException)
            {
                httpContext.Content = "Not found";
                statusCode = HttpStatusCode.NotFound;
            }
            catch (Exception exception)
            {
                httpContext.Content = exception.Message;
                statusCode = HttpStatusCode.InternalServerError;
            }

            return statusCode;
        }

        private HttpStatusCode Push(byte actionType, HttpServerContext httpContext, bool bin)
        {
            var body = httpContext.PostData;
            if (body == null || body.Length == 0)
            {
                httpContext.ResponseHeader["Content-Type"] = "text/html";
                httpContext.Content = "No body";
                return HttpStatusCode.BadRequest;
            }
            else if (httpContext.RequestHeader["Content-Type"] != "application/octet-stream")
            {
                httpContext.ResponseHeader["Content-Type"] = "text/html";
                httpContext.Content = "Please set content-type to application/octet-stream";
                return HttpStatusCode.BadRequest;
            }
            //
            if (bin)
            {
                httpContext.ResponseHeader["Content-Type"] = "application/octet-stream";
            }
            else
            {
                httpContext.ResponseHeader["Content-Type"] = "application/json";
            }
            httpContext.BeginWrite();
            //
            var queue = httpContext.QueryString["queue"] ?? "";
            var id = httpContext.QueryString["requestid"] ?? "";
            //
            var action = new PushAction();
            action.actionType = actionType;
            action.channel = new HttpState(httpContext, bin);
            action.queue = queue;
            action.id = id;
            action.item = new DataItem()
            {
                body = body
            };
            //
            Program.ActionProcessor.Push(action);
            //
            return HttpStatusCode.OK;
        }

        private HttpStatusCode Pull(HttpServerContext httpContext, bool bin)
        {
            if (bin)
            {
                httpContext.ResponseHeader["Content-Type"] = "application/octet-stream";
            }
            else
            {
                httpContext.ResponseHeader["Content-Type"] = "application/json";
            }
            httpContext.BeginWrite();
            //
            var queue = httpContext.QueryString["queue"] ?? "";
            var id = httpContext.QueryString["requestid"] ?? "";
            //
            var action = new PullAction();
            action.actionType = Action.PULL;
            action.channel = new HttpState(httpContext, bin);
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
            //
            return HttpStatusCode.OK;
        }

        private HttpStatusCode General(Action action, byte actionType, HttpServerContext httpContext, bool bin)
        {
            if (bin)
            {
                httpContext.ResponseHeader["Content-Type"] = "application/octet-stream";
            }
            else
            {
                httpContext.ResponseHeader["Content-Type"] = "application/json";
            }
            httpContext.BeginWrite();
            //
            var queue = httpContext.QueryString["queue"] ?? "";
            var id = httpContext.QueryString["requestid"] ?? "";
            //
            action.actionType = actionType;
            action.channel = new HttpState(httpContext, bin);
            action.queue = queue;
            action.id = id;
            //
            Program.ActionProcessor.Push(action);
            //
            return HttpStatusCode.OK;
        }

        private HttpStatusCode Home(HttpServerContext httpContext)
        {
            var queuename = httpContext.QueryString["queue"] ?? "";
            //
            httpContext.ResponseHeader["Content-Type"] = "text/html";
            httpContext.BeginWrite();
            //
            var action = new HomeAction();
            action.actionType = Action.HOME;
            action.channel = new HttpState(httpContext, false);
            action.queue = queuename;
            action.id = "";
            //
            Program.ActionProcessor.Push(action);
            //
            return System.Net.HttpStatusCode.OK;
        }

    }

    class HttpState : IChannel
    {
        private static readonly List<PullAction> pullActionLists = new List<PullAction>();

        private readonly HttpServerContext httpContext;
        private bool available = true;
        private bool bin = false;

        public HttpState(HttpServerContext context, bool bin)
        {
            this.httpContext = context;
            this.bin = bin;
        }

        public bool GetAvailable()
        {
            return this.available;
        }

        public void Disable()
        {
            this.available = false;
        }

        public List<PullAction> GetPulls()
        {
            return pullActionLists;
        }

        public void Send(Action action)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(s =>
            {
                if (action.actionType == Action.HOME)
                {
                    this.Home((HomeAction)action);
                }
                else if (this.bin == true)
                {
                    this.SendBin(action);
                }
                else
                {
                    this.SendJson(action);
                }
            });
        }

        private void SendBin(Action action)
        {
            var data = action.ToBytes();

            //
            try
            {
                this.httpContext.Write(data);
                this.httpContext.EndWrite();
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.available = false;
            }
            catch (InvalidOperationException)
            {
                this.available = false;
            }
            catch (Exception exception)
            {
                this.available = false;
                Program.LogManager.Exception(exception);
            }
        }

        private void SendJson(Action action)
        {
            var json = Action.ToJson(action);
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            try
            {
                this.httpContext.Write(data);
                this.httpContext.EndWrite();
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.available = false;
            }
            catch (InvalidOperationException)
            {
                this.available = false;
            }
            catch (Exception exception)
            {
                this.available = false;
                Program.LogManager.Exception(exception);
            }
        }

        private void Home(HomeAction action)
        {
            var hostname = HttpHandler.hostname;
            var version = HttpHandler.version;
            var servername = Program.ServiceContext.ServiceName;
            //
            var listBuild = new StringBuilder();
            //
            var propertyIndex = 0;
            QueueProperty[] queuePropertys = action.propertys;

            if (queuePropertys == null)
            {
                listBuild.AppendLine("<tbody>");
                listBuild.AppendLine("<tr>");
                listBuild.AppendLine("<td align=\"left\" colspan=\"4\" style=\"color:#ff8000;\">search timeout.</td>");
                listBuild.AppendLine("</tr>");
                listBuild.AppendLine("</tbody>");
            }
            else if (queuePropertys.Length == 0)
            {
                listBuild.AppendLine("<tbody>");
                listBuild.AppendLine("<tr>");
                listBuild.AppendLine("<td align=\"left\" colspan=\"4\" style=\"color:#ff8000;\">no queue.</td>");
                listBuild.AppendLine("</tr>");
                listBuild.AppendLine("</tbody>");
            }
            else
            {
                int l = 0;
                for (propertyIndex = 0, l = queuePropertys.Length; propertyIndex < l; propertyIndex++)
                {
                    var property = queuePropertys[propertyIndex];
                    if (property == null)
                    {
                        break;
                    }

                    listBuild.AppendLine("<tbody>");
                    listBuild.AppendLine("<tr>");
                    listBuild.AppendLine("<td align=\"left\">" + property.name + "</td>");
                    listBuild.AppendLine("<td align=\"center\">" + property.wait + "</td>");
                    listBuild.AppendLine("<td align=\"center\">" + property.pull + "</td>");
                    listBuild.AppendLine("<td align=\"center\">" + property.count + "</td>");
                    listBuild.AppendLine("</tr>");

                    listBuild.AppendLine("</tbody>");
                }
            }

            //
            var build = new StringBuilder();
            build.AppendLine("<!DOCTYPE html>");
            build.AppendLine("<html>");
            build.AppendLine("<head>");

            build.AppendLine("<style type=\"text/css\">");
            build.AppendLine(".tb1{ background-color:#D5D5D5;}");
            build.AppendLine(".tb1 td{ background-color:#FFF;}");
            build.AppendLine(".tb1 tr.None td{ background-color:#FFF;}");
            build.AppendLine(".tb1 tr.Success td{ background-color:#FFF;}");
            build.AppendLine(".tb1 tr.Failed td{ background-color:#FAEBD7;}");
            build.AppendLine(".tb1 tr.Running td{ background-color:#F5FFFA;}");
            build.AppendLine("img,form{ border:0px;}");
            build.AppendLine("img.button{ cursor:pointer; }");
            build.AppendLine("a { padding-left:5px; }");
            build.AppendLine("</style>");

            build.AppendLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\">");
            build.AppendLine("<title>" + servername + " Via " + hostname + "</title>");
            build.AppendLine("</head>");
            build.AppendLine("<body>");

            build.AppendLine("<div>");
            build.AppendLine("Powered by <a href=\"http://www.aooshi.org/adf\" target=\"_blank\">Adf.QueueServer</a> ");
            build.Append('v');
            build.Append(version.Major);
            build.Append(".");
            build.Append(version.Minor);
            build.Append(".");
            build.Append(version.Build);

            build.AppendLine(" Via " + hostname);

            build.AppendLine(" , Connection: <font color=\"green\">" + Program.ConnectionCounter + "</font>");
            build.AppendLine(" , Queue: <font color=\"green\" id=\"queuecount\">" + Program.QueueManager.Count + "</font>");
            build.AppendLine("</div>");

            build.AppendLine("<div>");
            build.AppendLine("<form action\"" + this.httpContext.Path + "\" method=\"get\">");
            build.AppendLine("<input type=\"text\" value=\"" + action.queue + "\" name=\"queue\" placeholder=\"queue name\" style=\"width:220px;\" />");
            build.AppendLine("<input type=\"submit\" value=\"list\" />");
            build.AppendLine("<input type=\"button\" value=\"home\" onclick=\"this.form['queue'].value='';this.form.submit();\" />");
            build.AppendLine("<input type=\"button\" value=\"create\" onclick=\"CreateQueue(this.form);\" />");
            build.AppendLine("<input type=\"button\" value=\"delete\" onclick=\"DeleteQueue(this.form);\" />");
            build.AppendLine("<input type=\"button\" value=\"clear\" onclick=\"ClearQueue(this.form);\" />");
            if (string.IsNullOrEmpty(action.queue))
            {
                build.AppendLine(" List random " + propertyIndex + " queue");
            }
            else
            {
                build.AppendLine(" Search " + action.queue + " result");
            }
            build.AppendLine("<span id=\"msg\"></span>");
            build.AppendLine("</form></div>");

            build.AppendLine("<table class=\"tb1\" width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"3\">");
            build.AppendLine("<thead>");
            build.AppendLine("<tr>");
            build.AppendLine("<th align=\"left\">Queue</th>");
            build.AppendLine("<th width=\"160\">Wait</th>");
            build.AppendLine("<th width=\"160\">Pull</th>");
            build.AppendLine("<th width=\"160\">Count</th>");
            build.AppendLine("</tr>");
            build.AppendLine("</thead>");

            build.Append(listBuild);

            build.AppendLine("</table>");


            build.AppendLine("<script type=\"text/javascript\" src=\"ajax.js\"></script>");
            build.AppendLine("<script type=\"text/javascript\" src=\"control.js\"></script>");
            build.AppendLine("</body>");
            build.AppendLine("</html>");

            try
            {
                this.httpContext.Write(build.ToString());
                this.httpContext.EndWrite();
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.available = false;
            }
            catch (InvalidOperationException)
            {
                this.available = false;
            }
            catch (Exception exception)
            {
                this.available = false;
                Program.LogManager.Exception(exception);
            }
        }
    }
}