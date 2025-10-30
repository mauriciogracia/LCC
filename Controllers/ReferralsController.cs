using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LCC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralsController : ControllerBase
    {
        /// <summary>
        /// referralCode = GetUniqueReferralCode (uid) 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetReferralCode")]
        public string GetReferralCode(string uid)
        {
            string code = string.Empty;

            return code;
        }
    }
    

    

}
