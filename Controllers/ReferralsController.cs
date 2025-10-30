using LCC.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralsController : ControllerBase
    {
        IReferralFeatures referralService;
        public ReferralsController(IReferralFeatures referral)
        {
            referralService = referral;
        }

        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetReferralCode")]
        public string GetReferralCode(string uid)
        {
            return referralService.GetUserReferralCode(uid);
        }
    }
    

    

}
