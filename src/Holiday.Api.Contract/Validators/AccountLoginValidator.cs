using FluentValidation;
using Holiday.Api.Contract.Dto;

namespace Holiday.Api.Contract.Validators;

public class AccountLoginValidator : AbstractValidator<AccountLoginDto>
{
    public AccountLoginValidator()
    {
        RuleFor(a => a.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress();

        RuleFor(a => a.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage("Le mot de passe ne peut pas être vide.")
            .Matches(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*\.!@$%^&\(\)\{\}\[\]\:;<>,\.?\~/_+\-=\|çÇ]).{8,32}$")
            .WithMessage("Votre mot de passe doit comporter entre 8 à 32 caractères, incluant au minimum un caractère spécial, une majuscule, une minuscule et un chiffre !");
    }
}