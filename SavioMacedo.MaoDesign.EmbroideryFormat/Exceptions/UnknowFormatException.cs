using System;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Exceptions
{
    public class UnknowFormatException: Exception
    {
       public UnknowFormatException(string message): base(message) { }
    }
}
