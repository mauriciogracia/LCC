using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class ReferralUpdateRequest
    {
        public string ReferralCode { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
