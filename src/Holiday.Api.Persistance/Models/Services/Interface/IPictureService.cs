using Microsoft.AspNetCore.Http;

namespace Holiday.Api.Repository.Models.Services.Interface;

public interface IPictureService
{
    string? UploadFile(IFormFile file);
    void deletePicture(string initialPath);
}