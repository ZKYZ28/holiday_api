using FluentValidation;
using Holiday.Api.Contract.Dto;

namespace Holiday.Api.Contract.Validators;

public class MailValidator : AbstractValidator<MailDto>
{
    public MailValidator()
    {
        RuleFor(a => a.SenderEmail)
            .NotNull()
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(a => a.Content)
            .NotEmpty().WithMessage("La description est obligatoire.")
            .Matches(@"^[A-Za-z\dÀ-ÿ\s,.!?;:\""<>[\]()\-+&'@#{}~$%ùçÇ=*]{5,1500}$")
            .WithMessage("La description peut contenir entre 5 et 1500 caractères et peut inclure des lettres, des chiffres, des apostrophes, des tirets, des espaces et certains caractères spéciaux.");

    }
}