using Domain;
using FluentValidation;

namespace Application.Validators
{
    public static class ReferralRules
    {
        public static IRuleBuilderOptions<T, string> ReferralCodeRules<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Referral code must not be empty.")
                .Length(6).WithMessage("Referral code must be exactly 6 characters.");
        }

        public static IRuleBuilderOptions<T, ReferralMethod> ReferralMethodRules<T>(this IRuleBuilder<T, ReferralMethod> rule)
        {
            return rule
                .IsInEnum().WithMessage("Invalid referral method.");
        }
    }
}
