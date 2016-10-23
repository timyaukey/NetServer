using System;
using System.Collections.Generic;

using Willowsoft.ServerLib;

namespace Willowsoft.ServerLib
{
    public interface IGenericServerUtilities
    {
        IDiagOutput DiagOutput { get; }
        void WriteDiagMessage(string msg);
        void WriteDiagException(Exception ex);
    }
}
