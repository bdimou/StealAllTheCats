using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators
{
    using FluentValidation;

    public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public PaginationRequestValidator()
        {
            RuleFor(x => x.PageIndex)
                .Must(value => int.TryParse(value, out var result) && result > 0)
                .WithMessage("Page index must be a non-negative integer.");

            RuleFor(x => x.PageSize)
                .Must(value => int.TryParse(value, out var result) && result > 0)
                .WithMessage("Page size must be a non-negative integer.");
        }
    }
}
