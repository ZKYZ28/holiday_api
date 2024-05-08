using System.Data;
using System.Text.RegularExpressions;
using DefaultNamespace;
using FluentValidation;

namespace Holiday.Api.Contract.Validators;

public class ActivityValidator : AbstractValidator<ActivityInDto>
{
    public ActivityValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Merci de spécifier un nom !")
            .Matches(@"[#\$%\*\+@\[\]\(\)A-Za-z0-9À-ÿ '\-,]{3,50}").WithMessage("Le nom doit contenir entre 3 et 50 caractères et peut inclure des lettres, des chiffres, des apostrophes, des tirets, des espaces et certains caractères spéciaux.");
        
        RuleFor(x => x.Description)
            .Must(desc => desc == null || desc.Length <= 500)
            .WithMessage("La descrption doit faire entre 0 ou 500 caracètres.")
            .Matches(@"^[A-Za-z\dÀ-ÿ\s,.!?;:\""<>[\]()\-+&'@#{}~$%ùçÇ=*]{0,500}$").When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("La description peut contenir de 0 et 500 caractères et peut inclure des lettres, des chiffres, des apostrophes, des tirets, des espaces et certains caractères spéciaux.");
        
        RuleFor(x => x.Price)
            .NotNull()
            .NotEmpty()
            .WithMessage("Le prix doit être défini.")
            .Must(p => Regex.IsMatch(p.ToString(), @"^\d+(,\d{1,2})?$"))
            .WithMessage(
                "Champ facultatif. Si vous choisissez de le remplir, veuillez saisir un nombre avec au maximum deux chiffres après le point en tant que séparateur décimal.");
        
        RuleFor(x => x.Location)
            .NotNull().WithMessage("Le lieu doit être défini !").SetValidator(new LocationValidator());
        
        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("La date de début doit être définie !")
            .LessThan(x => x.EndDate).WithMessage("La date de début doit être inférieur à la date de fin");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("La date de fin doit être définie !");
    }
    
    public class ActivityEditValidator : AbstractValidator<ActivityEditInDto>
    {
        public ActivityEditValidator()
        {
            // Inclure le validateur juste au dessus pour l'héritage
            Include(new ActivityValidator());
            RuleFor(x => x.DeleteImage)
                .NotNull();

            RuleFor(x => x.InitialPath)
                .NotEmpty(); 
        }
    }
}