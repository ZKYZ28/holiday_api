using FluentValidation;
using Holiday.Api.Contract.Dto;

namespace Holiday.Api.Contract.Validators;

public class HolidayValidator : AbstractValidator<HolidayInDto>
{
    public HolidayValidator()
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

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("La date de début doit être définie !")
            .LessThan(x => x.EndDate).WithMessage("La date de début doit être inférieure à la date de fin");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("La date de fin doit être définie !");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Le lieu doit être défini !").SetValidator(new LocationValidator());
        
        
    }
}

public class HolidayEditValidator : AbstractValidator<HolidayEditInDto>
{
    public HolidayEditValidator()
    {
        // Inclure le validateur juste au dessus pour l'héritage
        Include(new HolidayValidator());
        RuleFor(x => x.DeleteImage)
            .NotNull();

        RuleFor(x => x.InitialPath)
            .NotEmpty(); 
        
    }
}
