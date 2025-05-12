using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class IdValidator : AbstractValidator<string>
    {
        public IdValidator()
        {
            RuleFor(id => id)
                .NotEmpty().WithMessage("ID cannot be null or empty.")
                .Must(id => int.TryParse(id, out int parsed) && parsed > 0)
                .WithMessage("ID must be a valid positive integer.");
        }
    }

}
