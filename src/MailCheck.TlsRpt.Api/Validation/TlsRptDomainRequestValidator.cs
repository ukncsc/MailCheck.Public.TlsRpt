using FluentValidation;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Api.Domain;

namespace MailCheck.TlsRpt.Api.Validation
{
    public class TlsRptDomainRequestValidator : AbstractValidator<TlsRptDomainRequest>
    {
        public TlsRptDomainRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domains must be be a valid domain");
        }
    }
}
