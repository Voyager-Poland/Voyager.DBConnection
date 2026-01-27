using System;
using System.Collections.Generic;
using System.Text;

namespace Voyager.DBConnection.Exceptions
{

  [Serializable]
  public class LoggedExceptionException : Exception
  {
    public LoggedExceptionException() { }
    public LoggedExceptionException(string message) : base(message) { }
    public LoggedExceptionException(string message, Exception inner) : base(message, inner) { }

    public Boolean Logged { get; set; }
  }

}
