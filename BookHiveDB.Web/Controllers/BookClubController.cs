using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.Identity;
using BookHiveDB.Domain.Relations;
using BookHiveDB.Service.Interface;
using BookHiveDB.Domain.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class BookClubController : Controller
    {

        private readonly IBookClubService bookclubService;
        private readonly IUserService userService;
        private readonly ITopicService topicService;
        private readonly IInvitationService invitationService;
        private readonly UserManager<BookHiveApplicationUser> userManager;

        public BookClubController(IBookClubService bookclubService, IUserService userService, ITopicService topicService, IInvitationService invitationService, UserManager<BookHiveApplicationUser> userManager)
        {
            this.bookclubService = bookclubService;
            this.userService = userService;
            this.topicService = topicService;
            this.invitationService = invitationService;
            this.userManager = userManager;
        }

         public async Task<IActionResult> Index(string search, bool notUsed)
        {
            List<BookClub> bookClubs = null;
            if (search!=null && !search.Equals(""))
                bookClubs = bookclubService.findAllByNameContainingIgnoreCase(search);
            else
                bookClubs = bookclubService.findAll();
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            BookClubsLoggedInUser obj = new BookClubsLoggedInUser(bookClubs, user);
            return View(obj);
        }


        public async Task<IActionResult> Details(Guid? id)
        {
            BookClub bookClub = bookclubService.findById((Guid)id);
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            List<Topic> topicsInBookClub = topicService.findByBookClub(bookClub.Id);
            BookClubWithTopics dto = new BookClubWithTopics(bookClub, user, topicsInBookClub);
            return View(dto);
        }


        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string description) 
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            bookclubService.save(name, user.Id, description);
            return RedirectToAction(nameof(Index));
        }


   
        public IActionResult EditPage(Guid? id)
        {
            BookClub bookClub = bookclubService.findById((Guid)id);
            return View(bookClub);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, string name, string description) 
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User); 
            bookclubService.edit((Guid)id, name, user.Id, description);
            return RedirectToAction(nameof(Index));
        }

       
        public IActionResult DeletePage(Guid? id)
        {
            BookClub bookClub = bookclubService.findById((Guid)id);
            return View(bookClub);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBookClub(Guid id)
        {
            BookClub bookClub = bookclubService.findById((Guid)id);
            bookclubService.deleteById(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult removeMemberFromBookClub(string memberId, Guid id)
        {
            bookclubService.removeUserFromBookclub(id,memberId);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveBookClub(Guid id)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            bookclubService.removeUserFromBookclub(id, user.Id);
            return RedirectToRoute("default", new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> GetBookclubInvitationPage() 
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            return View(user);
        }

        public async Task<IActionResult> GetBookClubMembersPage(Guid id) 
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            BookClub bookClub = bookclubService.findById(id);
            List<BookHiveApplicationUser> members = bookclubService.getAllMembers(bookClub);
            BookClubMembersDTO dto = new BookClubMembersDTO(user, bookClub, members);
            return View(dto);
        }

        public async Task<IActionResult> GetBookclubMembershipRequestsPage(Guid id) 
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            BookClub bookClub = bookclubService.findById(id);
            List<Invitation> invitations = invitationService.findByBookClubAndIsRequest(id, true);
            BookClubInvitations dto = new BookClubInvitations(bookClub, invitations);
            return View(dto);
        }

        public async Task<IActionResult> RequestBookclubMembership(Guid id)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            BookClub bookClub = bookclubService.findById(id);
            invitationService.save(user.Id, bookClub.BookHiveApplicationUser.Email, id, "", true);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteTopic(Guid id)
        {
            Topic t = topicService.findById(id);
            topicService.deleteById(id);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = t.BookClubId });
        }
    }
}
