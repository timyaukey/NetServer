using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Willowsoft.ServerLib
{
    public interface IDiagOutput
    {
        void WriteMessage(string msg);
        void WriteException(Exception ex);
    }
}
