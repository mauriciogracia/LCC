using FluentValidation;
using Application.DTO;
using Application.Interfaces;
using Domain;

namespace Application.Validators
{
    public class UpdateReferralRequestValidator : AbstractValidator<UpdateReferralRequest>
    {
        public UpdateReferralRequestValidator(IUtilFeatures util)
        {
            RuleFor(x => x.ReferralCode).ReferralCodeRules(util);
            RuleFor(x => x.Name).NameRules();

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status cannot be empty.")
                .Must(s => Enum.TryParse<ReferralStatus>(s, true, out _))
                .WithMessage("Invalid referral status.");
        }
    }
}
