using CrossLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
  public interface ILogger
  {
    void LogException(Exception ex);

    void LogRequest(Request req, Exception ex);
  }

  public class Logger : ILogger
  {
    public void LogRequest(Request req, Exception ex)
    {
      using (StreamWriter sw = new("exceptions.log", true))
      {
        sw.WriteLine(string.Format("Command: {0}\n", req.Command));
        sw.WriteLine(string.Format("Data: {0}\n", req.Data));
        sw.WriteLine(string.Format("Exception: {0}\n", ex.Message));
        sw.WriteLine(string.Format("Help link: {0}\n", ex.HelpLink));
        sw.WriteLine("-----------------------------------------\n");
      }
    }

    public void LogException(Exception ex)
    {
      using (StreamWriter sw = new("exceptions.log", true))
      {
        sw.WriteLine(string.Format("Exception: {0}\n", ex.Message));
        sw.WriteLine(string.Format("Help link: {0}\n", ex.HelpLink));
        sw.WriteLine("-----------------------------------------\n");
      }
    }
  }
}