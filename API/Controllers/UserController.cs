using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// User Controller that exposes REST endpoints 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly ILog log;
        readonly IUserFeatures users;
        readonly IUtilFeatures util;

        public UserController(IUserFeatures usrs, IUtilFeatures utilService, ILog logger)
        {
            users = usrs;
            util = utilService;
            log = logger;
        }

        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("code/{uid}")]
        public async Task<ActionResult<string>> GetReferralCode(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return BadRequest("UID cannot be empty.");
            }

            var code = await users.GetUserReferralCode(uid);
            return Ok(code);
        }

        /// <summary>
        /// Validates that a referral code is valid
        /// </summary>
        /// <param name="referralCode"></param>
        /// <returns></returns>
        [HttpGet("validate/{referralCode}")]
        public bool ValidateReferralCode(string referralCode)
        {
            return util.IsValidReferralCode(referralCode);
        }


        /// <summary>
        /// Atributes that a referral has signed up and credits that to the stats of the user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("attribute")]
        public async Task<ActionResult<bool>> AttributeReferral([FromBody] ReferralAttributionRequest request)
        {
            var result = await users.AttributeReferral(request.ReferralCode, request.RefereeUid);
            return Ok(result);
        }
    }
}
