using System.Security;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models.Services.Interface;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Holiday.Api.Repository.Models;

public class PictureManager : IPictureService
{
    private readonly string _webRootPath;
    private readonly string _folderPicturePath;
    private const string FolderSave = "images";
    private const string DefaultFolderImage = "defaultImg";
    private const long maxFileSize = 5 * 1024 * 1024; // 5 Mo
    public static readonly string DefaultImageHoliday = Path.Combine(DefaultFolderImage, "logoTravel3.png");
    public static readonly string DefaultImageActivity = Path.Combine(DefaultFolderImage, "activity.png");
    public PictureManager(string webRootPath)
    {
        _webRootPath = webRootPath;
        _folderPicturePath = Path.Combine(webRootPath, FolderSave);
    }
    
    public string? UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }
        verifyFolderExists(_folderPicturePath);

        string? filePath = null;

        try
        {
            filePath = FilePath(file);
            
            ResizeAndSaveImage(file, filePath);
        }
        catch (HolidayStorageException e)
        {
            throw new HolidayStorageException(e.Message);
        }
        catch (ArgumentException)
        {
            throw new HolidayStorageException("Le fichier est invalide !");
        }
        catch (SecurityException)
        {
            throw new HolidayStorageException("Vous n'avez pas les permissions pour enregistrer le fichier");
        }
        catch (FileNotFoundException)
        {
            throw new HolidayStorageException("Le fichier n'existe pas");
        }
        catch (Exception)
        {
            throw new HolidayStorageException("Une erreur est survenue lors de l'enregistrement du fichier");
        }
        return GetImagePathForWeb(filePath);
    }

    /// <summary>
    /// Cette méthode va permettre de supprimer une image contenue dans le
    /// serveur d'images.
    /// Un cas bordure est à prendre en compte -> ne pas supprimer les images donnés par défaut.
    /// </summary>
    /// <param name="initialPath">chemin stocké en base de données</param>
    public void deletePicture(string initialPath)
    {
        if (initialPath.StartsWith(DefaultFolderImage))
        {
            return;
        }
        // wwwroot + images/... ou wwwroot + defaultImg/...
        string fullPath = Path.Combine(_webRootPath, initialPath);

        if (!File.Exists(fullPath))
        {
            throw new HolidayStorageException($"Le fichier avec le path {fullPath} n'a pas été trouvé ");
        }

        try
        {
            File.Delete(fullPath);
        }
        catch (PathTooLongException)
        {
            throw new HolidayStorageException($"Le chemin spécifié est trop long : ${fullPath}");
        }
        catch (NotSupportedException)
        {
            throw new HolidayStorageException($"Le chemin n'a pas un format valide : ${fullPath}");
        }
        catch (DirectoryNotFoundException)
        {
            throw new HolidayStorageException($"Le chemin spécifié n'a pas pu être trouvé : ${fullPath}");
        }
        catch (UnauthorizedAccessException)
        {
            throw new HolidayStorageException($"Vous n'avez pas les permissions de supprimer le fichier : ${fullPath}");
        }
        catch (IOException)
        {
            throw new HolidayStorageException(
                $"Une erreur est survenue lors de la suppresion du fichier : ${fullPath}");
        }
    }

    /// <summary>
    /// Redimensionne et enregistre une image dans le système de fichiers.
    /// </summary>
    /// <param name="file">Le fichier contenant l'image à redimensionner et à enregistrer.</param>
    /// <param name="destinationPath">Le chemin où l'image redimensionnée doit être enregistrée.</param>
    /// <remarks>
    /// Cette méthode prend en charge les formats d'image .jpg, .jpeg, et .png. Pour les images qui ont une largeur
    /// supérieure à 800 pixels, elle réduit la largeur à 800 pixels tout en conservant le rapport hauteur/largeur de l'image.
    /// Les images de format JPEG sont compressées pour réduire leur taille de fichier.
    /// </remarks>
    /// <exception cref="ArgumentException">Lancée si le fichier fourni n'est pas un format d'image supporté ou si les paramètres sont invalides.</exception>
    /// <exception cref="IOException">Lancée si une erreur se produit lors de la lecture, du redimensionnement ou de l'enregistrement de l'image.</exception
    private void ResizeAndSaveImage(IFormFile file, string destinationPath)
    {
        
        // OpenReadStream est fournie par l'interface IFormFile. Elle permet
        // de nous donner accès aux données binaires du fichier qui a été envoyée dans la requête http.
        using (var image = Image.Load(file.OpenReadStream()))
        {
            // Redimensionnement en conservant le ratio
            var maxWidth = 800;
            if (image.Width > maxWidth)
            {
                var newHeight = (int)(image.Height * maxWidth / (double)image.Width);
                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            // Compression
            var encoder = GetCompressionEncoder(file.FileName);
            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
            {
                image.Save(fileStream, encoder);
            }
        }
    }

    /// <summary>
    /// Permet de récupérer l'encodeur correspond au type d'images que l'on souhaite sauvegarder.
    /// C
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="HolidayStorageException"></exception>
    private IImageEncoder GetCompressionEncoder(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return new JpegEncoder
                {
                    Quality = 75,
                    
                };
            case ".png":
                // Compression, par défaut de niveau 6 : https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.html#SixLabors_ImageSharp_Formats_Png_PngCompressionLevel_DefaultCompression
                return new PngEncoder();
            default:
                throw new HolidayStorageException(
                    "Type d'image non pris en charge à la compression. Seulement .png, .jpg ou .jpeg");
        }
        
    }
    

    /// <summary>
    /// Cette méthode permet de générer un nom aléatoire au fichier pour éviter
    /// des caractères spéciaux non souhaités sur certains systèmes d'exploitation.
    /// De plus, elle vérifie l'extension du fichier pour éviter des attaques malveillantes.
    /// Elle vériie également la taille maximale de 5 Mo pour un fichier.
    /// Enfin, elle renverra le path où l'image doit aller s'enregister dans le serveur.
    /// </summary>
    /// <param name="file">Le fichier reçu depuis le front</param>
    /// <returns></returns>
    /// <exception cref="HolidayStorageException"></exception>
    private string FilePath(IFormFile file)
    {
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
        string? getExtensionFile = Path.GetExtension(file.FileName).ToLower();

        // Path.GetExtension peut renvoyer null ou Empty lorsqu'il ne trouve pas l'extension du fichier.
        if (string.IsNullOrEmpty(getExtensionFile) || !allowedExtensions.Contains(getExtensionFile))
        {
            throw new HolidayStorageException(
                "Extension de fichier non valide. Uniquement, les formats .png, .jpg ou .jpeg sont autorisés !");
        }
        
        if (file.Length > maxFileSize)
        {
            throw new HolidayStorageException("La taille du fichier dépasse la limite autorisée de 5 Mo.");
        }
        
        var fileName = $"{Path.GetRandomFileName()}{getExtensionFile}";
        return Path.Combine(_folderPicturePath, fileName);
    }

    /// <summary>
    /// Cette méthode permet de vérifier si le dossier pour enregister les images
    /// des utilisateurs existent bien sinon la méthode le crée.
    /// </summary>
    /// <param name="pathFolder"></param>
    /// <exception cref="HolidayStorageException"></exception>
    private void verifyFolderExists(string pathFolder)
    {
        try
        {
            if (!Directory.Exists(pathFolder))
            {
                Directory.CreateDirectory(pathFolder);
            }
        }
        catch (DirectoryNotFoundException)
        {
            throw new HolidayStorageException("Le chemin pointant vers le dossier n'a pu être trouvé");
        }
        catch (IOException)
        {
            throw new HolidayStorageException("Le dossier 'images' n'a pas pu être créé !");
        }
    }

    /// <summary>
    /// Permet d'extraire le path relatif du path absolu où l'image a été enregistrée.
    /// </summary>
    /// <param name="absoluteImagePath">Le chemin absolu où l'image a été enregistrée sur le serveur d'images.</param>
    /// <returns>une chaine de caractère représentant le chemin relatif où l'image a été stockée comme images/abc.png</returns>
    private string GetImagePathForWeb(string absoluteImagePath)
    {
        string relativePath = Path.GetRelativePath(_webRootPath, absoluteImagePath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

}