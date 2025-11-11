using Application.DTO;
using Application.Interfaces;
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

        public UserController(IUserFeatures userService, IUtilFeatures utilService, ILog logger)
        {
            users = userService;
            util = utilService;
            log = logger;
        }

        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("code/{uid}")]
        public async Task<ActionResult<ApiResponse<string>>> GetReferralCode([FromRoute] string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return BadRequest("UID cannot be empty.");
            }

            ApiResponse<string> resp = new ApiResponse<string>
            {
                Success = true,
                Data = await users.GetUserReferralCode(uid),
            };

            return Ok(resp);
        }

        /// <summary>
        /// Validates that a referral code is valid
        /// </summary>
        /// <param name="referralCode"></param>
        /// <returns></returns>
        [HttpGet("validate")]
        public ActionResult<ApiResponse<bool>> ValidateReferralCode([FromQuery] string code)
        {
            bool isValid = util.IsValidReferralCode(code);

            var resp = new ApiResponse<bool>
            {
                Success = isValid,
                Data = isValid,
            };

            return Ok(resp);
        }


        /// <summary>
        /// Attribute that a referred persona has signed up and credit that to the stats of the user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("{uid}/stats")]
        public async Task<ActionResult<ApiResponse<bool>>> AttributeReferral([FromRoute] string uid, [FromBody] ReferralAttributionRequest request)
        {
            var result = await users.AttributeReferral(request.ReferralCode, uid);

            var resp = new ApiResponse<bool>
            {
                Success = result,
                Data = result,
            };

            return Ok(resp);
        }
    }
}
