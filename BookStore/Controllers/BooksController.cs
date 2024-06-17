using AutoMapper;
using BookStore.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BookStore.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IUnitOfwork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public BooksController(IUnitOfwork unitOfWork, IMapper mapper, Cloudinary cloudinary)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        public IActionResult GetBooks(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? bookName = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? categoryId = null)
        {
            try
            {
                Expression<Func<Book, bool>> filter = b =>
                    (string.IsNullOrEmpty(bookName) || b.BookName.Contains(bookName)) &&
                    (!minPrice.HasValue || b.UnitPrice >= minPrice.Value) &&
                    (!maxPrice.HasValue || b.UnitPrice <= maxPrice.Value) &&
                    (!categoryId.HasValue || b.CategoryId == categoryId.Value);

                var books = _unitOfWork.BookRepo.Get(
                    filter: filter,
                    includeProperties: "Category,Images",
                    pageIndex: pageIndex,
                    pageSize: pageSize
                );

                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            try
            {
                Book book = _unitOfWork.BookRepo.GetById(id, includeProperties: "Category,Images");
                if (book == null)
                {
                    return NotFound(new { message = "Book Not Found" });
                }

                var bookDto = _mapper.Map<BookDto>(book);

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateBook([FromForm] BookMapper bookMapper)
        {
            try
            {
                if (!ModelState.IsValid || bookMapper.Images == null || bookMapper.Images.Count == 0)
                {
                    return BadRequest("Invalid data or no images provided.");
                }

                var imageUrls = new List<string>();
                foreach (var file in bookMapper.Images)
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

                var bookEntity = _mapper.Map<Book>(bookMapper);

                bookEntity.CategoryId = bookMapper.CategoryId;
                bookEntity.Images = new List<Image>();

                int nextImageId = _unitOfWork.ImageRepo.Get().Any()
                    ? _unitOfWork.ImageRepo.Get().Max(i => i.ImageId) + 1
                    : 1;

                foreach (var imageUrl in imageUrls)
                {
                    bookEntity.Images.Add(new Image { ImageId = nextImageId++, Url = imageUrl });
                }

                var lastBook = _unitOfWork.BookRepo.Get().LastOrDefault();
                bookEntity.BookId = (lastBook?.BookId ?? 0) + 1;

                _unitOfWork.BookRepo.Add(bookEntity);
                _unitOfWork.Save();
                int categoryID = (int)bookEntity.CategoryId;
                // Load the Category entity to ensure it is mapped correctly
                var category = _unitOfWork.CategoryRepo.GetById(categoryID);
                if (category == null)
                {
                    return BadRequest("Invalid CategoryId.");
                }
                bookEntity.Category = category;

                var bookDto = _mapper.Map<BookDto>(bookEntity);
                bookDto.Images = _mapper.Map<List<ImageDto>>(bookEntity.Images);
                bookDto.Category = _mapper.Map<CategoryDto>(bookEntity.Category); // Ensure the category is mapped

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }










    }
}
