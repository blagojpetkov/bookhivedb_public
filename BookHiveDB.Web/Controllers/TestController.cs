using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.Identity;
using BookHiveDB.Domain.Relations;
using BookHiveDB.Repository.Interface;
using BookHiveDB.Repository.Startup;
using BookHiveDB.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly IAuthorService authorService;
        private readonly IBookService bookService;
        private readonly IUserBookService userBookService;
        private readonly IOrderService orderService;
        private readonly GenreInitializer genreInitializer;
        private readonly IRepository<EmailMessage> EmailRepository;
        private readonly IShoppingCartService shoppingCartService;
        private readonly IUserRepository userRepository;
        private readonly IBookShopRepository bookShopRepository;


        private readonly RoleManager<IdentityRole> roleManager;

        public TestController(RoleManager<IdentityRole> roleManager, IBookShopRepository bookShopRepository, IUserRepository userRepository, IShoppingCartService shoppingCartService, IAuthorService authorService, IBookService bookService, IUserBookService userBookService, IOrderService orderService, GenreInitializer genreInitializer, IRepository<EmailMessage> EmailRepository)
        {
            this.roleManager = roleManager;
            this.bookShopRepository = bookShopRepository;
            this.userRepository = userRepository;
            this.authorService = authorService;
            this.bookService = bookService;
            this.userBookService = userBookService;
            this.orderService = orderService;
            this.genreInitializer = genreInitializer;
            this.EmailRepository = EmailRepository;
            this.shoppingCartService = shoppingCartService;
        }
        public IActionResult Home()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var roleExist = await roleManager.RoleExistsAsync("Administrator");
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole("Administrator"));
            }


            BookHiveApplicationUser user = userRepository.Get("12b20786-08b1-47e1-97e1-10a45e66d045");

            Author author = authorService.add("nameofauthor", "surnameofauthor", 55, "some author biography added");
            Author author2 = authorService.add("OTHERAUTHOR", "surnameofauthor", 55, "some author biography added");

            authorService.edit(author.Id, author.name, author.surname, 1000, author.biography);
            List<Author> authors = authorService.findAll();
            Author author1 = authorService.findById(author.Id);
            Book book = bookService.findAll()[0];
            book.name = "Testing book name";
            bookService.Update(book);

            var bookshops = bookShopRepository.getAllByBooks(book);

            List<Guid> listAuthorGuids = new List<Guid>();
            listAuthorGuids.Add(author1.Id);
            listAuthorGuids.Add(author2.Id);

            List<Genre> genres = new List<Genre>();
            Genre genre = new Genre { GenreName = "RomanceTEST" };
            Genre genre2 = new Genre { GenreName = "ActionTEST" };
            genres.Add(genre);genres.Add(genre2);
            genreInitializer.Insert(genre);
            genreInitializer.Insert(genre2);

            Book book2 = bookService.add("1234isbn123", "newbookname", "new book desc", "https://someaddress.com", DateTime.Now, listAuthorGuids, genres);
            BookShop bookShop = new BookShop { name = "Test bookshop" };
            List<BookInBookShop> bookInBookShops = new List<BookInBookShop>();
            bookInBookShops.Add(new BookInBookShop { Book = book2, BookId = book2.Id, BookShop = bookShop, BookShopId = bookShop.Id });
            bookShop.BookInBookShops = bookInBookShops;
            bookShopRepository.Insert(bookShop);



            author1.authorBooks.Add(new AuthorBook { Author = author1, AuthorId = author1.Id, Book = book, BookId = book.Id});

            authorService.Update(author1);

            Author testAuthor = authorService.findById(author.Id);

            bookService.AddToShoppingCart(new Domain.DTO.AddToShoppingCartDto { Book = book2, BookId = book2.Id, Quantity = 5 }, user.Id);
            shoppingCartService.order("12b20786-08b1-47e1-97e1-10a45e66d045");

            return View();
        }
    }
}
