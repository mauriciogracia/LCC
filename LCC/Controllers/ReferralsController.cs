using LCC.Interfaces;
using LCC.Models;
using Microsoft.AspNetCore.Mvc;

namespace LCC.Controllers
{
    /// <summary>
    /// In real life all this method should have error handling (try/catch) and asyn calls since some sort of repository/database will be used
    /// </summary>
    [Route("api/")]
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
        [HttpGet("referral-code/{uid}")]
        public ActionResult<string> GetReferralCode(string uid)
        {
            var code = referralService.GetUserReferralCode(uid);
            return Ok(code);
        }

        [HttpGet("referrals/{uid}")]
        public ActionResult<IEnumerable<Referral>> GetReferrals(string uid)
        {
            var referrals = referralService.GetUserReferrals(uid);
            return Ok(referrals);
        }

        [HttpPost("referrals")]
        public ActionResult<bool> AddReferral([FromBody] ReferralAddRequest request)
        {
            bool result = referralService.AddReferral(new Referral( request.Uid, request.Name, request.Method));
            return Ok(result);
        }
    }
}
