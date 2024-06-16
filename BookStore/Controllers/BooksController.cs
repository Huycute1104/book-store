using AutoMapper;
using BookStore.Model;
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

        public BooksController(IUnitOfwork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    includeProperties: "Category",
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
    }
}
