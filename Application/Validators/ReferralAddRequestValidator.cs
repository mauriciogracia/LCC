using FluentValidation;
using Application.DTO;

namespace Application.Validators
{
    public class ReferralAddRequestValidator : AbstractValidator<ReferralAddRequest>
    {
        public ReferralAddRequestValidator()
        {
            RuleFor(x => x.Uid)
                .NotEmpty().WithMessage("Uid cannot be null or empty");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be null or empty");

            RuleFor(x => x.ReferralCode).ReferralCodeRules();
            RuleFor(x => x.Method).ReferralMethodRules();
        }
    }
}
