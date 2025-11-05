using FluentValidation;
using Application.DTO;
using Application.Interfaces;

namespace Application.Validators
{
    public class ReferralAddRequestValidator : AbstractValidator<ReferralAddRequest>
    {
        public ReferralAddRequestValidator(IUtilFeatures util)
        {
            RuleFor(x => x.Uid)
                .NotEmpty().WithMessage("Uid cannot be null or empty");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be null or empty");

            RuleFor(x => x.ReferralCode).ReferralCodeRules(util);
            RuleFor(x => x.Method).ReferralMethodRules();
        }
    }
}
