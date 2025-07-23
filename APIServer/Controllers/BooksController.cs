using APIServer.DTO.Book;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using APIServer.Services;

namespace APIServer.Controllers.OData
{
    [Route("odata/[controller]")]
    public class BooksController : ODataController
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        

       
    }


}