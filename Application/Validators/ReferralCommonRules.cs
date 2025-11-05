using Domain;
using FluentValidation;
using Application.Interfaces;

namespace Application.Validators
{
    public static class ReferralCommonRules
    {
        public static IRuleBuilderOptions<T, string> ReferralCodeRules<T>(
            this IRuleBuilder<T, string> rule,
            IUtilFeatures util)
        {
            return rule
                .NotEmpty().WithMessage("Referral code must not be empty.")
                .Length(6).WithMessage("Referral code must be exactly 6 characters long.")
                .Must(util.IsValidReferralCode)
                .WithMessage("Referral code failed domain-level validation.");
        }

        public static IRuleBuilderOptions<T, ReferralMethod> ReferralMethodRules<T>(
            this IRuleBuilder<T, ReferralMethod> rule)
        {
            return rule
                .IsInEnum().WithMessage("Invalid referral method.");
        }

        public static IRuleBuilderOptions<T, string> UidRules<T>(
            this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Uid cannot be null or empty");
        }

        public static IRuleBuilderOptions<T, string> NameRules<T>(
            this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Name cannot be null or empty");
        }
    }
}
