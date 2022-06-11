using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Model
{
    public class Logger
    {
        private static ILog _logger;
        private static FileAppender _fileAppender;
        private static RollingFileAppender _rollingFileAppender;
        private static String _layout = "%date{dd-MMM-yyyy-HH:mm:sss} [%level] [%class] [%method] - %message%newline";

        public static string Layout
        {
            set { _layout = value; }
        }

        private static PatternLayout GetPatternLayout()
        {
            var patternLayout = new PatternLayout()
            {
                ConversionPattern = _layout
            };
            patternLayout.ActivateOptions();

            return patternLayout;
        }
        private static RollingFileAppender GetRoolingFileAppender()
        {
            var roolingfileAppender = new RollingFileAppender()
            {
                Name = "Rolling File Appender",
                Layout = GetPatternLayout(),
                Threshold = Level.All,
                AppendToFile = true,
                File = "logs\\",
                MaximumFileSize = "5MB",
                DatePattern = "yyyy-MM-dd.'txt'",
                StaticLogFileName = false,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaxSizeRollBackups = 31,
            };
            roolingfileAppender.ActivateOptions();
            return roolingfileAppender;
        }

        public static ILog GetLog(Type type)
        {
            if (_rollingFileAppender == null)
                _rollingFileAppender = GetRoolingFileAppender();
            if (_logger != null)
                return _logger;
            BasicConfigurator.Configure(_rollingFileAppender);
            _logger = LogManager.GetLogger(type);
            return _logger;
        }

    }
}
