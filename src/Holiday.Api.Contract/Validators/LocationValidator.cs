using DefaultNamespace;
using FluentValidation;
using Holiday.Api.Contract.Dto;
using Microsoft.AspNet.SignalR.Messaging;

namespace Holiday.Api.Contract.Validators;

public class LocationValidator: AbstractValidator<LocationDto>
{
    public LocationValidator()
    {
        RuleFor(x => x.Country)
            .NotNull()
            .NotEmpty()
            .Length(3, 50).WithMessage("Le pays doit être défini entre 3 à 50 caractères.")
            .Matches(@"[A-Za-z\dÀ-ÿ '\-,]{3,50}").WithMessage(
                "Veuillez entrer un nom de pays valide entre 3 et 50 caractères. Les lettres, chiffres, apostrophes, tirets et espaces sont autorisés.");
        
        RuleFor(x => x.Locality)
            .NotNull()
            .NotEmpty()
            .Length(3, 100).WithMessage("La localité doit être défini entre 3 à 100 caractères.")
            .Matches(@"[A-Za-z\dÀ-ÿ .'\-,]{3,100}").WithMessage(
                "Veuillez saisir une localité valide contenant entre 3 et 100 caractères. Seules les lettres, chiffres, espaces, apostrophes, points, virgules et tirets sont autorisés.");
        
        RuleFor(x => x.PostalCode)
            .NotNull()
            .NotEmpty()
            .Length(1, 15).WithMessage("Le code postal doit être défini entre 1 à 15 caractères.")
            .Matches(@"[A-Za-z\d\-, ]{1,15}").WithMessage(
                "Veuillez saisir un code postal valide entre 1 à 15 caractères. Exemples : 4000, 75000.");
        
        RuleFor(x => x.Street)
            .Length(3, 100).When(x => !string.IsNullOrEmpty(x.Street))
            .WithMessage("Champ faculatif. Si la rue est définie, celle-ci doit être définie entre 3 à 100 caractères.")
            .Matches(@"^[A-Za-zÀ-ÿ\d ',.-]{3,100}").When(x => !string.IsNullOrEmpty(x.Street))
            .WithMessage("Champ faculatif. Si vous choisissez de le remplir, veuillez saisir une adresse de rue valide contenant entre 3 et 100 caractères. Seules les lettres, chiffres, espaces, apostrophes, points, virgules et tirets sont autorisés.");

        RuleFor(x => x.Number)
            .Matches(@"^[A-Za-z\d\- ]{1,10}$").When(x => !string.IsNullOrEmpty(x.Number)).WithMessage(
                "Champ faculatif. Si vous choisissez de le remplir, veuillez entrer un numéro de boîte valide entre 1 à 10 caractères. Exemples : 77, 77A, PO Box 123, PMB 456B.");

    }
}