using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace AmazonBookReleaseTracker
{
    public class AmazonIdValidator : AbstractValidator<AmazonId>
    {
        public AmazonIdValidator()
        {
            RuleFor(id => id.Asin)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Must(AmazonId.IsValid).WithMessage("{PropertyName} must be a valid ASIN.");
        }
    }
}
