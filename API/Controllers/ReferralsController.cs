using Application.DTO;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetReferralCode(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                var msg = $"Invalid uid: {uid}";
                log.error(msg);
                return BadRequest(msg);
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
            string errorMsg = request.validate();

            //When validation error is found on the ReferralAddRequest
            if (!string.IsNullOrEmpty(errorMsg))
            {
                log.error(errorMsg);
                return BadRequest(errorMsg);
            }

            bool isValidCode = util.IsValidReferralCode(request.ReferralCode);

            if (!isValidCode)
            {
                var msg = $"Invalid referral code: {request.ReferralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

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
        public ActionResult<string> PrepareMessage([FromQuery] string method, [FromQuery] string referralCode)
        {
            if (!Enum.TryParse<ReferralMethod>(method, true, out var rm))
            {
                var msg = $"Invalid referral method: {method}";
                log.error(msg);
                return BadRequest(msg);
            }

            bool isValidCode = util.IsValidReferralCode(referralCode);

            if (!isValidCode)
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }
            var inviteMessage = util.PrepareMessage(rm, referralCode);

            return Ok(inviteMessage);
        }

        [HttpGet("referrals/{referralCode}")]
        public async Task<ActionResult<Referral>> GetReferral(string referralCode, string name)
        {
            bool isValidCode = util.IsValidReferralCode(referralCode);

            if (!isValidCode)
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            var referral = await referrals.GetReferral(referralCode, name);
            return Ok(referral);
        }

        [HttpPut("referrals/{referralCode}")]
        public async Task<ActionResult<bool>> UpdateReferral(string referralCode, [FromQuery] string name, [FromQuery] string status)
        {
            if (!Enum.TryParse<ReferralStatus>(status, true, out var rs))
            {
                var msg = $"Invalid referral status: {status}";
                log.error(msg);
                return BadRequest(msg);
            }

            bool isValidCode = util.IsValidReferralCode(referralCode);

            if (!isValidCode)
            {
                var msg = $"Invalid referral code: {referralCode}";
                log.error(msg);
                return BadRequest(msg);
            }

            bool succeeded = await referrals.UpdateReferral(referralCode, name, rs);

            return Ok(succeeded);
        }
    }
}
