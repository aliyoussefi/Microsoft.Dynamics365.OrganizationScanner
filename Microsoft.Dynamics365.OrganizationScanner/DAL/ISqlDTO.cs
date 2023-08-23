using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    public interface ISqlDTO
    {
        public string SqlCommand { get; set; }
        public string CorrelatonId { get; set; }
    }
}
