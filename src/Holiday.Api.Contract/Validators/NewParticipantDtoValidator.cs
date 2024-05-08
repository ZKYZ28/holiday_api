using FluentValidation;
using Holiday.Api.Contract.Dto;

namespace Holiday.Api.Contract.Validators;

public class NewParticipantDtoValidator : AbstractValidator<NewParticipantDto>
{
    public NewParticipantDtoValidator()
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

        RuleFor(a => a.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Votre prénom doit être défini !")
            .Matches(@"[a-zA-ZÀ-ÿ][çÇ\-\.a-z' ]{1,28}[a-zÀ-ÿ]")
            .WithMessage(
                "Veuillez sasir un nom valide, il ne doit pas être vide, ne doit pas inclure de chiffres et doit comporter entre 3 et 28 caractères.");
        
        RuleFor(a => a.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Votre nom doit être défini !")
            .Matches(@"[a-zA-ZÀ-ÿ][çÇ\-\.a-z' ]{1,48}[a-zÀ-ÿ]")
            .WithMessage(
                "Veuillez sasir un nom valide, il ne doit pas être vide, ne doit pas inclure de chiffres et doit comporter entre 3 et 48 caractères.");
    }
}