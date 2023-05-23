using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Repository;
using BookHiveDB.Service.Interface;
using BookHiveDB.Repository.Startup;
using BookHiveDB.Domain.Relations;
using BookHiveDB.Domain.DTO;
using System.Security.Claims;

namespace BookHiveDB.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService bookService;
        private readonly IAuthorService authorService;
        private readonly GenreInitializer genreInitializer;
        public BooksController(IBookService bookService, IAuthorService authorService, GenreInitializer genreInitializer)
        {
            this.authorService = authorService;
            this.bookService = bookService;
            this.genreInitializer = genreInitializer;
        }

        // GET: Books
        public IActionResult Index()
        {
            return View(bookService.findAll());
        }


        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["authors"] = authorService.findAll();
            ViewData["genres"] = genreInitializer.GetAll();
            return View();
        }


        // GET: Books/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = bookService.findById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }


        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("isbn,name,description,datePublished,coverImageUrl,Price,isValid,Id")] Book book, List<Guid> authors, List<Guid> genres)
        {
            if (ModelState.IsValid)
            {
                book.Id = Guid.NewGuid();
                book.authorBooks = new List<AuthorBook>();
                book.BookGenres = new List<BookGenre>();
                foreach (var authorGuid in authors)
                {
                    var author = authorService.findById(authorGuid);
                    book.authorBooks.Add(new Domain.Relations.AuthorBook { Author = author, AuthorId = author.Id, Book = book, BookId = book.Id });
                }
                foreach (var genreGuid in genres)
                {
                    var genre = genreInitializer.GetById(genreGuid);
                    book.BookGenres.Add(new Domain.Relations.BookGenre {Genre = genre, GenreId = genre.Id, Book = book, BookId = book.Id });
                }
                bookService.CreateNewBook(book);
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public IActionResult Edit(Guid? id)
        {
            ViewData["authors"] = authorService.findAll();
            ViewData["genres"] = genreInitializer.GetAll();
            if (id == null)
            {
                return NotFound();
            }

            var book = bookService.findById(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("isbn,name,description,datePublished,coverImageUrl,Price,isValid,Id")] Book book, List<Guid> authors, List<Guid> genres)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Book book1 = bookService.findById(book.Id);
                    book1.isbn = book.isbn;
                    book1.name = book.name;
                    book1.description = book.description;
                    book1.datePublished = book.datePublished;
                    book1.coverImageUrl = book.coverImageUrl;
                    book1.Price = book.Price;
                    book1.isValid = book.isValid;



                    book1.authorBooks = new List<AuthorBook>();
                    book1.BookGenres = new List<BookGenre>();
                    foreach (var authorGuid in authors)
                    {
                        var author = authorService.findById(authorGuid);
                        book1.authorBooks.Add(new Domain.Relations.AuthorBook { Author = author, AuthorId = author.Id, Book = book, BookId = book.Id });
                    }
                    foreach (var genreGuid in genres)
                    {
                        var genre = genreInitializer.GetById(genreGuid);
                        book1.BookGenres.Add(new Domain.Relations.BookGenre { Genre = genre, GenreId = genre.Id, Book = book, BookId = book.Id });
                    }
                    bookService.Update(book1);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = bookService.findById(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            bookService.deleteById(id);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult AddBookToCart(Guid id)
        {
            var result = this.bookService.GetShoppingCartInfo(id);

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBookToCart(AddToShoppingCartDto model)
        {


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = this.bookService.AddToShoppingCart(model, userId);

            if (result)
            {
                return RedirectToAction("Index", "Books");
            }
            return View(model);
        }

        private bool BookExists(Guid id)
        {
            return bookService.findById(id) != null;
        }
    }
}
