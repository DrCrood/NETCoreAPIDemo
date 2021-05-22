using System;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NetCoreAPI.Services
{
    public interface IServiceLog
    {
        bool Configure();

        bool SendCourseTrainingEmailNotice(string address, string name, List<string> trainings, string subject, string message);
        bool Log(Level level, string message, string action = "", bool succeed = true, Exception exception = null, string mailsubject = "");
        bool SetConnectionString(string connString);
        bool SetEnvironment(string env);
    }

    public enum Level
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    class ServiceLog : IServiceLog
    {
        IConfiguration _appConfig;
        LoggingConfiguration logConfig;
        //FileTarget errorFile;
        ConsoleTarget consoleTarget;
        DatabaseTarget dbTarget;
        MailTarget mailTarget;

        NLog.Logger logger;

        private string Action;
        private bool Succeed;
        private string Environment = "Canvas";
        private string EmailSubject = "";

        public ServiceLog(IConfiguration configuration)
        {
            _appConfig = configuration;
            logConfig = new LoggingConfiguration();
            this.Configure();
        }

        public bool Configure()
        {
            LogManager.Setup().SetupExtensions(s => s.RegisterLayoutRenderer("action", (logevent) => Action));
            LogManager.Setup().SetupExtensions(s => s.RegisterLayoutRenderer("succeed", (logevent) => Succeed.ToString()));
            LogManager.Setup().SetupExtensions(s => s.RegisterLayoutRenderer("subject", (logevent) => EmailSubject));
            LogManager.Setup().SetupExtensions(s => s.RegisterLayoutRenderer("environment", (logevent) => Environment));

            String date = DateTime.Now.ToString("yyyy-MM-dd");

            consoleTarget = new ConsoleTarget("consoleLog")
            {
                DetectConsoleAvailable = true,
                Layout = "${longdate} ${level} ${message} ${exception}"
            };

            mailTarget = new MailTarget("emailLog")
            {
                Subject = "SynService Running Notice @${environment}: ${subject}",
                To = "",
                CC = "",
                Body = "Automatic Email Notice from WorkerService:  ${newline}${newline} ${message} ${newline} ${newline} ${exception}",
                From = "",
                SmtpAuthentication = SmtpAuthenticationMode.None,
                SmtpServer = "",
                SmtpPort = 25,
                EnableSsl = true
            };

            dbTarget = new DatabaseTarget("dbLog")
            {
                DBProvider = "Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient",
                //ConnectionString = connectStr,
                CommandType = System.Data.CommandType.StoredProcedure,
                CommandText = "spApp_InsertWorkerServiceLogs",
                //CommandText = @"INSERT INTO WorkerServiceLogs (Logtime, Level, Action, Succeed, Message, Exception)
                //                VALUES(@logTime, @level, @action, @succeed, @message, @exception )",
            };

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@logTime", "${date}"));
            dbTarget.Parameters.Add(new DatabaseParameterInfo("@level", "${level}"));
            dbTarget.Parameters.Add(new DatabaseParameterInfo("@action", "${action}"));
            dbTarget.Parameters.Add(new DatabaseParameterInfo("@succeed", "${succeed}"));
            dbTarget.Parameters.Add(new DatabaseParameterInfo("@message", "${message}"));
            dbTarget.Parameters.Add(new DatabaseParameterInfo("@exception", "${exception}"));

            logConfig.AddRule(LogLevel.Trace, LogLevel.Info, consoleTarget, "Microsoft.*", true);
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);
            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, dbTarget);
            logConfig.AddRule(LogLevel.Fatal, LogLevel.Fatal, mailTarget);

            LogManager.Configuration = logConfig;
            logger = LogManager.GetCurrentClassLogger();
            return true;
        }

        public bool SendCourseTrainingEmailNotice(string address, string name, List<string> trainings, string subject, string message)
        {
            string msgBody = "<p>Howdy FULLNAME,</p>" +
                             "<p>This is a auto notice to let you know you have tranining need to be completed before you can ...</p>" +
                             "<p>TRAININGS</p>" +
                             "<p>COURSENAME</p>" +
                             "<p>BODYTEXT</p>" +
                             "<p><b>Thank you!</b></P>";
            msgBody = msgBody.Replace("FULLNAME", name);
            msgBody = msgBody.Replace("COURSENAME", "course name here");
            msgBody = msgBody.Replace("BODYTEXT", message);

            string trainingList = "";
            int t = 1;
            foreach (string training in trainings)
            {
                trainingList += "<p><b>" + t + ". " + training + "</b></p>";
                t++;
            }
            msgBody = msgBody.Replace("TRAININGS", trainingList);

            using (var smtpClient = new SmtpClient("server name"))
            {
                MailMessage mail = new MailMessage("from", address, subject, msgBody);
                mail.IsBodyHtml = true;
                smtpClient.Send(mail);
            }

            return true;
        }

        public bool Log(Level level, string message, string action = "", bool succeed = true, Exception exception = null, string mailsubject = "")
        {
            Action = action;
            Succeed = succeed;
            EmailSubject = mailsubject;
            switch (level)
            {
                case Level.Trace:
                    logger.Trace(message);
                    break;
                case Level.Debug:
                    logger.Debug(message);
                    break;
                case Level.Info:
                    logger.Info(message);
                    break;
                case Level.Warn:
                    logger.Warn(message);
                    break;
                case Level.Error:
                    if (exception != null)
                    {
                        logger.Error(exception, message);
                    }
                    else
                    {
                        logger.Error(message);
                    }
                    break;
                case Level.Fatal:
                    if (exception != null)
                    {
                        logger.Fatal(exception, message);
                    }
                    else
                    {
                        logger.Fatal(message);
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool SetConnectionString(string connstring)
        {
            dbTarget.ConnectionString = connstring;
            return true;
        }

        public bool SetEnvironment(string env)
        {
            this.Environment = env;
            return true;
        }

    }
}
