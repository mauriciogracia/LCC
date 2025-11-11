using FluentValidation;
using Application.DTO;

namespace Application.Validators
{
    public class ReferralAttributionRequestValidator : AbstractValidator<ReferralAttributionRequest>
    {
        public ReferralAttributionRequestValidator()
        {
            RuleFor(x => x.RefereeUid).UidRules();
            RuleFor(x => x.ReferralCode).NotEmpty().WithMessage("Referral code is required.");
        }
    }
}
