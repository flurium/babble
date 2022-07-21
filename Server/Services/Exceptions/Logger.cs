using CrossLibrary;

namespace Server.Services.Exceptions
{
    public static class Logger
    {
        static Logger()
        {
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
        }

        public static void LogTransaction(Transaction req, Exception ex)
        {
            using (StreamWriter sw = new("logs\\exceptions.log", true))
            {
                sw.WriteLine(string.Format("Command: {0}", req.Command));
                sw.WriteLine(string.Format("Data: {0}", req.Data));
                sw.WriteLine(string.Format("Exception: {0}", ex.Message));
                sw.WriteLine("---------------------------------------------------");
            }
        }

        public static void LogException(Exception ex)
        {
            using (StreamWriter sw = new("logs\\exceptions.log", true))
            {
                sw.WriteLine(string.Format("Exception: {0}", ex.Message));
                sw.WriteLine(string.Format("Help link: {0}", ex.HelpLink));
                sw.WriteLine("---------------------------------------------------");
            }
        }
    }
}