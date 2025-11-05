using FluentValidation;
using Application.DTO;
using Application.Interfaces;

namespace Application.Validators
{
    public class PrepareMessageRequestValidator : AbstractValidator<PrepareMessageRequest>
    {
        public PrepareMessageRequestValidator(IUtilFeatures util)
        {
            RuleFor(x => x.Method).ReferralMethodRules();
            RuleFor(x => x.ReferralCode).ReferralCodeRules(util);
        }
    }
}
