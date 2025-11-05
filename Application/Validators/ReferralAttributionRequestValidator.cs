using FluentValidation;
using Application.DTO;

namespace Application.Validators
{
    public class ReferralAttributionRequestValidator : AbstractValidator<ReferralAttributionRequest>
    {
        public ReferralAttributionRequestValidator()
        {
            RuleFor(x => x.ReferralCode)
                .NotEmpty().WithMessage("Referral code is required.");

            RuleFor(x => x.RefereeUid)
                .NotEmpty().WithMessage("Referee UID is required.");
        }
    }
}
