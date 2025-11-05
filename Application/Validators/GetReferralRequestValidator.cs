using FluentValidation;
using Application.DTO;
using Application.Interfaces;

namespace Application.Validators
{
    public class GetReferralRequestValidator : AbstractValidator<GetReferralRequest>
    {
        public GetReferralRequestValidator(IUtilFeatures util)
        {
            RuleFor(x => x.ReferralCode)
                .ReferralCodeRules(util);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty.");
        }
    }
}
