using System;
using System.Diagnostics;
using System.IO;

[assembly:log4net.Config.XmlConfigurator]
[assembly: log4net.Config.Repository]

namespace LiveSequence.Common
{
    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    public class Logger
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger("Logging");
		
        private static volatile Logger _current;
        private static readonly object syncRoot = new object();

        private Logger()
        {}

        public static Logger Current
        {
            get
            {
                if (_current == null)
                {
                    lock(syncRoot)
                    {
                        if (_current == null)
                        {
                            _current = new Logger();
                        }
                    }
                }

                return _current;
            }
        }

        public void Debug(string message)
        {		
            if (this.log.IsDebugEnabled)
                //this.log.Debug("[" + DateTime.Now.ToString() + "] " + ExtractInfo(message));
                this.log.Debug(message);
        }

        public void Info(string message)
        {			
            if (this.log.IsInfoEnabled)
                this.log.Info("[" + DateTime.Now + "] " + ExtractInfo(message));
        }

        public void Error(string message, Exception e)
        {
            if (this.log.IsErrorEnabled)
                this.log.Error("[" + DateTime.Now + "] " + ExtractInfo(message), e);
        }

        public void Error(string message)
        {
            if (this.log.IsErrorEnabled)
                this.log.Error("[" + DateTime.Now + "] " + ExtractInfo(message));
        }

        private static string ExtractInfo(string message)
        {
            StackFrame frame1 = new StackFrame(2, true);
            string methodName = frame1.GetMethod().ToString();
            string fileName = Path.GetFileName(frame1.GetFileName());
            string[] textArray1 = new[] { "File:", fileName, " - Method:", methodName, " - ", message } ;

            return string.Concat(textArray1);
        }
    }
}