//NOTICE: © 2022 The Regents of the University of Michigan

//The Chuck Mayo Lab - https://medicine.umich.edu/dept/radonc/research/research-laboratories/physics-laboratories 

//The software is solely for non-commercial, non-clinical research and education use in support of the publication.
//It is a decision support tool and not a surrogate for professional clinical guidance and oversight.
//The software calls APIs that are owned by Varian Medical Systems, Inc. (referred to here as Varian),
//and you should be aware you will need to obtain an API license from Varian in order to be able to use those APIs.
//Extending the [No Liability] aspect of these terms, you agree that as far as the law allows,
//Varian and Michigan will not be liable to you for any damages arising out of these terms or the use or nature of the software,
//under any kind of legal claim, and by using the software, you agree to indemnify the licensor and Varian in the event that the
//licensor or Varian is joined in any lawsuit alleging injury or death or any other type of damage (e.g., intellectual property infringement)
//arising out of the dissemination or use of the software.

using System;
using System.IO;
using System.Text;

namespace AnalyticsLibrary2
{

    public static class Log3_static // An exact copy of Log and Logger2 in AP_lib project. Added here to improvie logging in AnalyticsLibrary project.
    {
        private static logger3 logger;

        public static void Warning(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.warn);
            }
        }

        public static void Error(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.error);
            }
        }

        public static void Debug(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.debug);
            }
        }


        public static void Information(string msg)
        {
            if (logger != null)
            {
                logger.WriteLine(msg, Log_levels.info);
            }
        }

        public static void initiate_logger3(string filePath, Log_levels minimal_level = Log_levels.debug)
        {
            logger = new logger3(filePath, minimal_level);
        }

    }

    public class logger3
    {
        private string DatetimeFormat = "yyyy-MM-dd HH-mm-ss.fff";
        private string Filename;



        public logger3(string path_filename, Log_levels lv = Log_levels.debug)
        {
            Filename = path_filename;
            level_min = lv;
        }

        private readonly Log_levels level_min;

        internal void WriteLine(string text, Log_levels level = Log_levels.debug)
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
