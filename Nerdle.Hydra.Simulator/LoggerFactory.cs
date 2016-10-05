using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Nerdle.Hydra.Simulator
{
    class LoggerFactory
    {
        static int _index = -1;
        static readonly ColoredConsoleAppender.Colors[] Colors = 
        {
            ColoredConsoleAppender.Colors.Green,
            ColoredConsoleAppender.Colors.Red,
            ColoredConsoleAppender.Colors.Yellow,
            ColoredConsoleAppender.Colors.Purple,
            ColoredConsoleAppender.Colors.Cyan,
        };

        public static ILog CreateLogger(string id)
        {
            var log = LogManager.GetLogger(id);

            var appender = CreateAppender();
            MapColors(appender);
            appender.ActivateOptions();

            var logger = (Logger)log.Logger;
            logger.AddAppender(appender);

            return log;
        }

        static ColoredConsoleAppender CreateAppender()
        {
            var layout = new PatternLayout("%date [%thread] %-5level %message%newline");
            layout.ActivateOptions();

            var appender = new ColoredConsoleAppender
            {
                Layout = layout
            };
            
            return appender;
        }

        static void MapColors(ColoredConsoleAppender appender)
        {
            var index = Interlocked.Increment(ref _index);
            var color = Colors[index % Colors.Length];

            var infoColours = new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Info,
                ForeColor = color,
            };

            var errorColours = new ColoredConsoleAppender.LevelColors
            {
                Level = Level.Error,
                ForeColor = ColoredConsoleAppender.Colors.White & ColoredConsoleAppender.Colors.HighIntensity,
                BackColor = color
            };

            infoColours.ActivateOptions();
            errorColours.ActivateOptions();

            appender.AddMapping(infoColours);
            appender.AddMapping(errorColours);
        }
    }
}