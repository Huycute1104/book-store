using AutoMapper;
using BookStore.Model;
using CloudinaryDotNet;
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
        public IActionResult CreateBook([FromBody] BookMapper bookMapper)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Code to create a book...
                // (This part is incomplete in the provided code)

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
