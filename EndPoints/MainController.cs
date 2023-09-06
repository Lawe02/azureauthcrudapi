using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Azure;
using FunctionsApi.Models;
using FunctionsApi.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using webapi.User;
using static System.Net.WebRequestMethods;


namespace FunctionsApi.EndPoints
{
    public class MainController
    {
        private readonly ILogger _logger;
        private readonly LoginService _loginService;
        private readonly BookService _bookService;

        public MainController(ILoggerFactory loggerFactory, LoginService loginService, BookService bookService)
        {
            _bookService = bookService;
            _logger = loggerFactory.CreateLogger<MainController>();
            _loginService = loginService;
        }
        public List<Book> Books { get; set; }

        [Function("Login")]
        public async Task<HttpResponseData> LoginAsync(
   [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
   FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", MediaTypeNames.Application.Json);

            var userLogin = await req.ReadFromJsonAsync<UserLogin>();

            if(userLogin == null)
            {
                response.WriteString("Ivalid req body");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var user = _loginService.Authenticate(userLogin);

            if(user != null)
            {
                var token = _loginService.Generate(user);
                var jsonResponse = new { token };
                var jsonString = JsonSerializer.Serialize(jsonResponse);  

                //response.WriteString(jsonString);
                await response.WriteAsJsonAsync(jsonResponse);
                _logger.LogInformation(token.ToString());
                return response;
            }

            _logger.LogInformation("User not found, gutter!!!");
            response.StatusCode=HttpStatusCode.NotFound;
            response.WriteString("User not found");

            return response;
        }
        [Function("GetBooks")]
        public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            if(_loginService.IsAuthenticated(req.Headers.GetValues("Authorization").FirstOrDefault()))
            {
                Books = _bookService.RetrieveBooks();
                _bookService.SaveBooks(Books);
                await response.WriteAsJsonAsync(Books);
            }
            else
            {
                _logger.LogInformation("bad req");   
            }
           
            return response;
        }

        [Function("DeleteBook")]
        public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequestData req, FunctionContext executionContext)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            if (_loginService.IsAuthenticated(req.Headers.GetValues("Authorization").FirstOrDefault()))
            {
                Guid.TryParse(req.ReadAsString(), out var bookId);

                if (bookId != null)
                {
                    Books = _bookService.RetrieveBooks();

                    var bookToDelete = Books.FirstOrDefault(x => x.BookId == bookId);

                    if (bookToDelete != null)
                    {
                        Books.Remove(bookToDelete);
                        response.WriteString($"Book : Title: {bookToDelete.Title} - Author: {bookToDelete.Author}. Deleted succesfully!");
                        _bookService.SaveBooks(Books);
                        return response;
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.WriteString("Book not found");
                        return response;
                    }
                }
                else
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }
            else
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }
        }

        [Function("CreateBook")]
        public async Task<HttpResponseData> CreateAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", MediaTypeNames.Application.Json);

            var book = await req.ReadFromJsonAsync<Book>();

            if (_loginService.IsAuthenticated(req.Headers.GetValues("Authorization").FirstOrDefault()))
            {
                Books = _bookService.RetrieveBooks();
                Books.Add(book);
                _bookService.SaveBooks(Books);
                response.WriteString($" Book : Title {book.Title} : Author : {book.Author} , added succesfully");

                return response;
            }
            else
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }
        }

        [Function("UpdateBook")]
        public async Task<HttpResponseData> UpdateAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", MediaTypeNames.Application.Json);

            var bookToUpdateProps = await req.ReadFromJsonAsync<Book>();

            if(_loginService.IsAuthenticated(req.Headers.GetValues("Authorization").FirstOrDefault()))
            {
                Books = _bookService.RetrieveBooks();
                var bookToUpdate = Books.FirstOrDefault(x => x.BookId == bookToUpdateProps.BookId);

                bookToUpdate.Title = bookToUpdateProps.Title;
                bookToUpdate.Author = bookToUpdateProps.Author;
                bookToUpdate.PublicityDate = bookToUpdateProps.PublicityDate;

                _bookService.SaveBooks(Books);
                return response;
            }
            else 
            { 
                response.StatusCode = HttpStatusCode.Unauthorized; 
                return response; 
            }

        }
        [Function("test")]
        public async Task<HttpResponseData> TestAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("SABJASBJKASBKJSABJKSA");
            return response;
        }

    }
}
