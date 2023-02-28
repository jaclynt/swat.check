using System.Diagnostics;

namespace SWAT.Check.Helpers;

public class ErrorUtility
{
    public static string GetExceptionMessages(Exception e)
    {
        const string exceptionMessageFormat = "Type: {0}\r\nMessage: {1}\r\nSource: {2}\r\nLocation: {3}\r\nStackTrace: \r\n{4}\r\n\r\n";

        string exceptionMessage = string.Empty;

        while (e != null)
        {
            StackTrace st = new StackTrace(e, true);
            string errorLocation = string.Empty;
            try
            {
                errorLocation = string.Format(
                    "{0}, line {1}, column {2}",
                    st.GetFrame(0).GetMethod().ReflectedType.FullName,
                    st.GetFrame(0).GetFileLineNumber(),
                    st.GetFrame(0).GetFileColumnNumber());
            }
            catch (Exception) { }

            exceptionMessage += string.Format(exceptionMessageFormat, e.GetType(), e.Message, e.Source, errorLocation, e.StackTrace);
            e = e.InnerException;
        }

        return (exceptionMessage);
    }
}
