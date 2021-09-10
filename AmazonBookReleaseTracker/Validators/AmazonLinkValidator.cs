using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace AmazonBookReleaseTracker
{
    public class AmazonLinkValidator : AbstractValidator<AmazonLink>
    {
        public AmazonLinkValidator()
        {
            RuleFor(id => id.Link)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Must(AmazonLink.IsValidUri).WithMessage("{PropertyName} must be a valid URL.");
        }
    }
}