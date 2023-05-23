using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.Identity;
using BookHiveDB.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class InvitationController : Controller
    {
        private readonly IInvitationService invitationService;
        private readonly IUserService userService;
        private readonly IBookClubService bookclubService;
        private readonly UserManager<BookHiveApplicationUser> userManager;

        public InvitationController(IInvitationService invitationService, IUserService userService, IBookClubService bookclubService, UserManager<BookHiveApplicationUser> userManager)
        {
            this.invitationService = invitationService;
            this.userService = userService;
            this.bookclubService = bookclubService;
            this.userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InvitationPage(Guid bookClubId)
        {
            BookClub bookClub = bookclubService.findById(bookClubId);
            return View(bookClub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendInvitation(Guid bookClubId, string senderId, string receiverEmail, string message)
        {
            invitationService.save(senderId, receiverEmail, bookClubId, message, false);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id=bookClubId });
        }

        public async Task<IActionResult> GetMyInvitations()
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            List<Invitation> invitations = this.invitationService.findByReceiver(user.Email);
            return View(invitations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvitation(Guid id)
        {
            Invitation invitation = invitationService.findById(id);
            bookclubService.addUserToBookclub(invitation.BookClubId, invitation.UserReceiverId);
            this.invitationService.deleteById(id);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = invitation.BookClubId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclineInvitation(Guid id)
        {
            Invitation invitation = invitationService.findById(id);
            invitationService.deleteById(id);
            return RedirectToRoute("default", new { controller = "User", action = "Index" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptRequest(Guid id)
        {
            Invitation invitation = invitationService.findById(id);
            bookclubService.addUserToBookclub(invitation.BookClubId, invitation.UserSenderId);
            this.invitationService.deleteById(id);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = invitation.BookClubId });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeclineRequest(Guid id)
        {
            Invitation invitation = invitationService.findById(id);
            invitationService.deleteById(id);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = invitation.BookClubId });
        }
    }
}
