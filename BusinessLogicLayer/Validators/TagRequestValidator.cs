using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class TagRequestValidator : AbstractValidator<TagRequest>
    {
        public TagRequestValidator()
        {
            RuleFor(x => x.Name)
                .Must(BeAlpha).WithMessage("Tag name must only contain alphabetic characters.");
        }

        private bool BeAlpha(string value)
        {
            return value.All(char.IsLetter);
        }
    }
}
