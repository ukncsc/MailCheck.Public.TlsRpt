using System.Collections.Generic;
using FluentValidation;
using MailCheck.Common.Util;

namespace MailCheck.TlsRpt.Reports.Api.Controllers
{
    public class DomainDateRangeRequestValidator : AbstractValidator<DomainDateRangeRequest>
    {
        public DomainDateRangeRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domain must be be a valid domain");

            RuleFor(x => x.Period)
                .Must(x => new List<string> { "2-days", "14-days", "90-days" }.Contains(x))
                .WithMessage("Period in days must be one of '2-days', '14-days', '90-days'");
        }
    }
}