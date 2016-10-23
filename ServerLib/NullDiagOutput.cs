using System;

namespace Willowsoft.ServerLib
{
    public class NullDiagOutput : IDiagOutput
    {
        public void WriteMessage(string msg)
        {
        }

        public void WriteException(Exception ex)
        {
        }
    }
}
