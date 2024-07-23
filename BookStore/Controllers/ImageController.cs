using AutoMapper;
using BookStore.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using Repository.Models;
using Repository.UnitOfwork;
using System.Collections.Generic;
using System.Linq;

namespace BookStore.Controllers
{
    [Route("api/images")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly Cloudinary _cloudinary;

        public ImageController(IUnitOfwork unitOfWork, Cloudinary cloudinary)
        {
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteImage(int id)
        {
            var image = _unitOfWork.ImageRepo.GetById(id);
            if (image == null)
            {
                return NotFound();
            }

            _unitOfWork.ImageRepo.Delete(image);
            _unitOfWork.Save();

            return Ok();
        }

        [HttpPost]
        public IActionResult UploadImages([FromForm] UpdateBookImages updateBookImages)
        {
            if (updateBookImages.Images == null || !updateBookImages.Images.Any())
            {
                return BadRequest("No files uploaded.");
            }

            var imageUrls = new List<string>();

            try
            {
                var book = _unitOfWork.BookRepo.GetById(updateBookImages.BookId);
                if (book == null)
                {
                    return NotFound("Book not found.");
                }

                foreach (var file in updateBookImages.Images)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream())
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    if (uploadResult.Error != null)
                    {
                        throw new Exception(uploadResult.Error.Message);
                    }

                    imageUrls.Add(uploadResult.SecureUrl.AbsoluteUri);
                }

                var newImages = imageUrls.Select(url => new Image
                {
                    Url = url,
                    BookId = updateBookImages.BookId
                }).ToList();

                book.Images.AddRange(newImages);

                _unitOfWork.Save();

                return Ok(new { imageUrls });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error uploading images: {ex.Message}");
            }
        }
    }
}
