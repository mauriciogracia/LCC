using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Referral Controller that exposes REST endpoints 
    /// </summary>
    [Route("api/[controller]")]
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
        /// Gets the Referrals of a given user (uid)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("{uid}/referrals/")]
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> AddReferral([FromBody] ReferralAddRequest request)
        {
            //TODO MGG - use ApiResponse
            bool result = await referrals.AddReferral(new Referral(request.Uid, request.Name, request.Method, request.ReferralCode));
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="referralCode"></param>
        /// <returns></returns>
        [HttpGet("invite-message")]
        public ActionResult<ApiResponse<string>> PrepareMessage([FromQuery] PrepareMessageRequest pmr)
        {
            //TODO MGG - use ApiResponse
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

        [HttpGet]
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

        [HttpPut("{referralId}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateReferral(string referralId, [FromBody] ReferralUpdateRequest rur)
        {
            bool success;

            Enum.TryParse<ReferralStatus>(rur.Status, true, out var rs);
            success = await referrals.UpdateReferral(rur.ReferralCode, rur.Name, rs);

            ApiResponse<bool> resp = new ApiResponse<bool>
            {
                Success = success,
                Data = success
            };

            return Ok(resp);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<ReferralStatistics>> GetReferralStats([FromQuery] string uid)
        {
            var stats = await referrals.GetReferralStatistics(uid);
            return Ok(stats);
        }
    }
}
