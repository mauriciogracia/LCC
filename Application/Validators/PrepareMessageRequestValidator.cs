using FluentValidation;
using Application.DTO;

namespace Application.Validators
{
    public class PrepareMessageRequestValidator : AbstractValidator<PrepareMessageRequest>
    {
        public PrepareMessageRequestValidator()
        {
            RuleFor(x => x.Method).ReferralMethodRules();
            RuleFor(x => x.ReferralCode).ReferralCodeRules();
        }
    }
}
