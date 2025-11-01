using LCC.Interfaces;
using LCC.Models;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetReferralCode(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                var msg = $"Invalid uid: {uid}";
                log.error(msg);
                return BadRequest(msg);
            }
            try
            {
                var code = await referralService.GetUserReferralCode(uid);
                return Ok(code);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
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

            try
            {
                var referrals = await referralService.GetUserReferrals(uid);
                return Ok(referrals);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
            
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

            try
            {
                bool isValidCode = await referralService.IsValidReferralCode(request.ReferralCode);

                if (!isValidCode)
                {
                    var msg = $"Invalid referral code: {request.ReferralCode}";
                    log.error(msg);
                    return BadRequest(msg);
                }

                bool result = await referralService.AddReferral(new Referral(request.Uid, request.Name, request.Method, request.ReferralCode));
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="referralCode"></param>
        /// <returns></returns>
        [HttpGet("referrals/invite-msg")]
        public async Task<ActionResult<string>> PrepareMessage([FromQuery] string method, [FromQuery] string referralCode)
        {
            if (!Enum.TryParse<ReferralMethod>(method, true, out var rm))
            {
                var msg = $"Invalid referral method: {method}";
                log.error(msg);
                return BadRequest(msg);
            }

            try
            {
                bool isValidCode = await referralService.IsValidReferralCode(referralCode);

                if (!isValidCode)
                {
                    var msg = $"Invalid referral code: {referralCode}";
                    log.error(msg);
                    return BadRequest(msg);
                }
                var inviteMessage = await referralService.PrepareMessage(rm, referralCode);

                return Ok(inviteMessage);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("referrals/{referralCode}")]
        public async Task<ActionResult<Referral>> GetReferral(string referralCode, string name)
        {
            try
            {
                bool isValidCode = await referralService.IsValidReferralCode(referralCode);

                if (!isValidCode)
                {
                    var msg = $"Invalid referral code: {referralCode}";
                    log.error(msg);
                    return BadRequest(msg);
                }

                var referral = await referralService.GetReferral(referralCode,  name);
                return Ok(referral);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
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

            try
            {
                bool isValidCode = await referralService.IsValidReferralCode(referralCode);

                if (!isValidCode)
                {
                    var msg = $"Invalid referral code: {referralCode}";
                    log.error(msg);
                    return BadRequest(msg);
                }

                bool succeeded = await referralService.UpdateReferral(referralCode, name, rs);

                return Ok(succeeded);
            }
            catch (Exception ex)
            {
                log.error(ex.Message);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
