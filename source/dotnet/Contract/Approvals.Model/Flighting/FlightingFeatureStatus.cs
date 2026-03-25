using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CFS.Approvals.Model.Flighting
{
    public class FlightingFeatureStatusEntity : BaseTableEntity
    {
        public string FeatureStatus { get; set; }
        public int FeatureStatusID { get; set; }
    }
}
