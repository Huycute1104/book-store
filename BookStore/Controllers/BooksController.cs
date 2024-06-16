using AutoMapper;
using BookStore.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Models;
using Repository.UnitOfwork;
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
        public IActionResult GetBook(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
             

                var books = _unitOfWork.BookRepo.Get(
                    pageIndex: pageIndex,
                    pageSize: pageSize
                );

                var bookDtos = _mapper.Map<IEnumerable<BookMapper>>(books);

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
