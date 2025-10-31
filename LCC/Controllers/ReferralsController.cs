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
        [HttpGet("referrals/code/{uid}")]
        public ActionResult<string> GetReferralCode(string uid)
        {
            //TODO MGG - validate arguments and return BAD REQUEST
            var code = referralService.GetUserReferralCode(uid);
            return Ok(code);
        }

        [HttpGet("referrals/list/{uid}")]
        public ActionResult<IEnumerable<Referral>> GetReferrals(string uid)
        {
            //TODO MGG - validate arguments and return BAD REQUEST
            var referrals = referralService.GetUserReferrals(uid);
            return Ok(referrals);
        }

        [HttpPost("referrals")]
        public ActionResult<bool> AddReferral([FromBody] ReferralAddRequest request)
        {
            //TODO MGG - validate arguments and return BAD REQUEST
            bool result = referralService.AddReferral(new Referral( request.Uid, request.Name, request.Method, request.ReferralCode));
            return Ok(result);
        }

        [HttpGet("referrals/msg")]
        public ActionResult<string> PrepareMessage([FromQuery] string method, [FromQuery] string referralCode)
        {
            if (!Enum.TryParse<ReferralMethod>(method, true, out var rm))
            {
                return BadRequest($"Invalid referral method: {method}");
            }

            return Ok(referralService.PrepareMessage(rm, referralCode));
        }

        [HttpGet("referrals/{refCode}")]
        public ActionResult<string> GetReferral(string refCode)
        {
            //TODO MGG - validate arguments and return BAD REQUEST
            return Ok(referralService.GetReferral(refCode));
        }
    }
}
