using Application.DTO;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Referral Controller that exposes REST endpoints 
    /// </summary>
    [Route("api/")]
    [ApiController]
    public class ReferralsController : ControllerBase
    {
        readonly ILog log;
        readonly IReferralFeatures referrals;
        readonly IUtilFeatures util;

        public ReferralsController(IReferralFeatures referral, IUtilFeatures utilService, ILog logger)
        {
            referrals = referral;
            util = utilService;
            log = logger;
        }

        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("referrals/code/{uid}")]
        public async Task<ActionResult<string>> GetReferralCode(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return BadRequest("UID cannot be empty.");
            }

            var code = await referrals.GetUserReferralCode(uid);
            return Ok(code);
        }
        [HttpGet("referrals/validate/{referralCode}")]
        public bool ValidateReferralCode(string referralCode)
        {
            return util.IsValidReferralCode(referralCode);
        }

        /// <summary>
        /// Gets the Referrals of a given user (uid)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("referrals/list/{uid}")]
        public async Task<ActionResult<IEnumerable<Referral>>> GetReferrals(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                var msg = $"Invalid uid: {uid}";
                log.error(msg);
                return BadRequest(msg);
            }

            var referrals = await this.referrals.GetUserReferrals(uid);
            return Ok(referrals);
        }
        /// <summary>
        /// Adds a Referral to a user using the ReferralAddRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("referrals")]
        public async Task<ActionResult<bool>> AddReferral([FromBody] ReferralAddRequest request)
        {
            bool result = await referrals.AddReferral(new Referral(request.Uid, request.Name, request.Method, request.ReferralCode));
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="referralCode"></param>
        /// <returns></returns>
        [HttpGet("referrals/invite-msg")]
        public ActionResult<string> PrepareMessage([FromQuery] PrepareMessageRequest pmr)
        {
            bool isValidCode = util.IsValidReferralCode(pmr.ReferralCode);

            if (!isValidCode)
            {
                var msg = $"Invalid referral code: {pmr.ReferralCode}";
                log.error(msg);
                return BadRequest(msg);
            }
            var inviteMessage = util.PrepareMessage(pmr.Method, pmr.ReferralCode);

            return Ok(inviteMessage);
        }

        [HttpGet("referrals")]
        public async Task<ActionResult<Referral>> GetReferral([FromQuery] GetReferralRequest grr)
        {
            if (!util.IsValidReferralCode(grr.ReferralCode))
            {
                var msg = $"Invalid referral code: {grr.ReferralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            var referral = referrals.GetReferral(grr.ReferralCode, grr.Name);

            if (referral == null)
            {
                return NotFound();
            }

            return Ok(referral);
        }

        //ASP.NET Core sometimes fails to bind [FromQuery] complex objects in PUT requests — even when the query string is correct
        [HttpPut("referrals")]
        public async Task<ActionResult<bool>> UpdateReferral(
    [FromQuery] string referralCode,
    [FromQuery] string name,
    [FromQuery] string status)
        {
            Enum.TryParse<ReferralStatus>(status, true, out var rs);
            var succeeded = await referrals.UpdateReferral(referralCode, name, rs);
            return Ok(succeeded);
        }

        [HttpGet("referrals/stats")]
        public async Task<ActionResult<ReferralStatistics>> GetReferralStats([FromQuery] string uid)
        {
            var stats = await referrals.GetReferralStatistics(uid);
            return Ok(stats);
        }

        [HttpPost("attribute")]
        public async Task<ActionResult<bool>> AttributeReferral([FromBody] ReferralAttributionRequest request)
        {
            var result = await referrals.AttributeReferral(request.ReferralCode, request.RefereeUid);
            return Ok(result);
        }
    }
}
