using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    public interface IARConverterFactory
    {
        IARConverter GetARConverter();
    }
}
