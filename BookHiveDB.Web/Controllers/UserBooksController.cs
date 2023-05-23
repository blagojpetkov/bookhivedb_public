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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using BookHiveDB.Domain.Identity;

namespace BookHiveDB.Web.Controllers
{
    public class UserBooksController : Controller
    {

        private readonly IUserService userService;
        private readonly IUserBookService userBookService;
        private readonly IBookService bookService;


        public UserBooksController(IUserBookService userBookService, IUserService userService, IBookService bookService)
        {
            this.userBookService = userBookService;
            this.userService = userService;
            this.bookService = bookService;
        }

        public IActionResult getMyBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userService.findById(userId);
            return View(userBookService.FindByUser(user));
        }

        public IActionResult addToMyBooks(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userBookService.addBookToMyBooks(userId, id);
            return RedirectToAction("getMyBooks");
        }

        public IActionResult editLastPageRead(Guid bookId, int lastPage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userBookService.editLastPageReadForBook(userId, bookId, lastPage);

            BookHiveApplicationUser user = userService.findById(userId);
            Book book = bookService.findById(bookId);
            UserBook userbook = userBookService.FindByUserAndBook(user, book);

            if (lastPage > book.totalPages)
                userbook.lastPageRead = book.totalPages;
            else
                userbook.lastPageRead = lastPage;
            userBookService.Update(userbook);
            return RedirectToAction("getMyBooks");

        }

        public IActionResult DeletePage(Guid id)
        {
            UserBook obj = userBookService.Get(id);
            return View(obj);
        }

        public IActionResult DeleteConfirmed(Guid id)
        {
            UserBook obj = userBookService.Get(id);
            userBookService.RemoveFromMyBooks(id);
            return RedirectToAction("getMyBooks");
        }

    }
}
