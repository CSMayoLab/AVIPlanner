using System;
using System.IO;
using System.Text;

namespace AP_lib
{

    public static class Log 
    {
        public static logger2 logger;

        public static void Warning(string msg)
        {
            if (logger!= null)
            {
                logger.WriteLine(msg, Log_levels.warn);
            }
        }

        public static void initiate_logger2(string filePath, Log_levels minimal_level = Log_levels.debug)
        {
            logger = new logger2(filePath, minimal_level);
        }

    }

    public class logger2
    {
        private string DatetimeFormat = "yyyy-MM-dd HH-mm-ss.fff";
        private string Filename;

        

        public logger2(string path_filename, Log_levels lv = Log_levels.debug)
        {
            Filename = path_filename;
            level_min = lv;
        }

        private readonly Log_levels level_min;

        public void WriteLine(string text, Log_levels level = Log_levels.debug)
        {
            if (level < level_min) return;

            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    using (StreamWriter Writer = new StreamWriter(Filename, true, Encoding.UTF8))
                    {
                        Writer.WriteLine(DateTime.Now.ToString(DatetimeFormat) + "\t" + text);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Logging for SRS AutoStructure is not working properly.\n\nPlease notify developer.", e);
            }
        }
    }

    public enum Log_levels
    {
        debug,
        info,
        warn,
        error
    }
}
