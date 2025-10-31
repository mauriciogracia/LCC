using LCC.Interfaces;
using LCC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace LCC.Controllers
{
    /// <summary>
    /// Referral Controller that exposes REST endpoints 
    /// </summary>
    [Route("api/")]
    [ApiController]
    public class ReferralsController : ControllerBase
    {
        ILog log ;
        IReferralFeatures referralService;

        public ReferralsController(IReferralFeatures referral, ILog log)
        {
            referralService = referral;
            this.log = log;
        }

        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("referrals/code/{uid}")]
        public async Task<ActionResult<string>> GetReferralCode(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                var msg = $"Invalid uid: {uid}";
                log.error(msg);
                return BadRequest(msg);
            }
            var code = await referralService.GetUserReferralCode(uid);
            return Ok(code);
        }

        [HttpGet("referrals/list/{uid}")]
        public async Task<ActionResult<IEnumerable<Referral>>> GetReferrals(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                var msg = $"Invalid uid: {uid}";
                log.error(msg);
                return BadRequest(msg);
            }
            var referrals = await referralService.GetUserReferrals(uid);
            return Ok(referrals);
        }

        [HttpPost("referrals")]
        public async Task<ActionResult<bool>> AddReferral([FromBody] ReferralAddRequest request)
        {
            string errorMsg = request.validate();

            //When validation error is found on the ReferralAddRequest
            if (!string.IsNullOrEmpty(errorMsg))
            {
                log.error(errorMsg);
                return BadRequest(errorMsg);
            }

            if(!await referralService.IsValidReferralCode(request.ReferralCode))
            {
                var msg = $"Invalid referral code: {request.ReferralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            bool result = await referralService.AddReferral(new Referral( request.Uid, request.Name, request.Method, request.ReferralCode));
            return Ok(result);
        }

        [HttpGet("referrals/invite-msg")]
        public async Task<ActionResult<string>> PrepareMessage([FromQuery] string method, [FromQuery] string referralCode)
        {
            if (!Enum.TryParse<ReferralMethod>(method, true, out var rm))
            {
                var msg = $"Invalid referral method: {method}";
                log.error(msg);
                return BadRequest(msg);
            }

            if (!await referralService.IsValidReferralCode(referralCode))
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            return Ok(await referralService.PrepareMessage(rm, referralCode));
        }

        [HttpGet("referrals/{referralCode}")]
        public async Task<ActionResult<string>> GetReferral(string referralCode)
        {
            if (!await referralService.IsValidReferralCode(referralCode))
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            return Ok(referralService.GetReferral(referralCode));
        }

        [HttpPut("referrals/{referralCode}")]
        public async Task<ActionResult<string>> UpdateReferral(string referralCode, [FromQuery] string status)
        {
            if (!Enum.TryParse<ReferralStatus>(status, true, out var rs))
            {
                var msg = $"Invalid referral status: {status}";
                log.error(msg);
                return BadRequest(msg);
            }

            if (!await referralService.IsValidReferralCode(referralCode))
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            return Ok(referralService.UpdateReferral(referralCode, rs));
        }
    }
}
