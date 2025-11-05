using FluentValidation;
using Application.DTO;
using Application.Interfaces;

namespace Application.Validators
{
    public class ReferralAddRequestValidator : AbstractValidator<ReferralAddRequest>
    {
        public ReferralAddRequestValidator(IUtilFeatures util)
        {
            RuleFor(x => x.Uid).UidRules();
            RuleFor(x => x.Name).NameRules();
            RuleFor(x => x.ReferralCode).ReferralCodeRules(util);
            RuleFor(x => x.Method).ReferralMethodRules();
        }
    }
}
