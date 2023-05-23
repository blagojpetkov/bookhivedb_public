using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.DTO;
using BookHiveDB.Domain.Identity;
using BookHiveDB.Domain.Relations;
using BookHiveDB.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class UserController : Controller
    {

        private readonly IBookInWishListService bookInWishListService;
        private readonly IUserBookService userBookService;
        private readonly IBookClubService bookClubService;
        private readonly UserManager<BookHiveApplicationUser> userManager;
        private readonly IBookService bookService;

        public UserController(IBookInWishListService bookInWishListService, IUserBookService userBookService, IBookClubService bookClubService, UserManager<BookHiveApplicationUser> userManager, IBookService bookService)
        {
            this.bookInWishListService = bookInWishListService;
            this.userBookService = userBookService;
            this.bookClubService = bookClubService;
            this.userManager = userManager;
            this.bookService = bookService;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetMyBookClubs()
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            List<BookClub> myBookClubs = bookClubService.findByMember(user.Id);
            BookClubsLoggedInUser obj = new BookClubsLoggedInUser(myBookClubs, user);
            return View(obj);
        }

        public IActionResult LeaveBookClubPage(Guid bookClubId)
        {
            BookClub obj = bookClubService.findById(bookClubId);
            return View(obj);
            
        }
        public async Task<IActionResult> RemoveBookClubFromMyBookClubsAsync(Guid bookClubId)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            bookClubService.removeUserFromBookclub(bookClubId, user.Id);
            return RedirectToAction(nameof(GetMyBookClubs));
        }

        public async Task<IActionResult> AddBookToMyWishList(Guid bookId)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            bookInWishListService.addBookToMyWishlist(user.Id, bookId);
            return RedirectToAction(nameof(GetMyWishList));
        }
        public IActionResult RemoveBookPage(Guid bookId)
        {
            Book obj = bookService.findById(bookId);
            return View(obj);
        }

        public async Task<IActionResult> RemoveBookFromMyWishList(Guid bookId)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            bookInWishListService.removeBookFromMyWishlist(user.Id, bookId);
            return RedirectToAction(nameof(GetMyWishList));
        }

        public async Task<IActionResult> GetMyWishList()
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            List<BookInWishList> myWishList = bookInWishListService.getAllBookInWishlistForUser(user.Id);
            return View(myWishList);
        }

    }
}
