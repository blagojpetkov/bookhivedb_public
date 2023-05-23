using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.Identity;
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
    public class TopicController : Controller
    {
        private readonly ITopicService topicService;
        private readonly IPostService postService;
        private readonly IUserService userService;
        private readonly IBookClubService bookClubService;
        private readonly UserManager<BookHiveApplicationUser> userManager;

        public TopicController(ITopicService topicService, IPostService postService, IUserService userService, UserManager<BookHiveApplicationUser> userManager, IBookClubService bookClubService)
        {
            this.topicService = topicService;
            this.postService = postService;
            this.userService = userService;
            this.userManager = userManager;
            this.bookClubService = bookClubService;
        }

        public async Task<IActionResult> GetTopicById(Guid id)
        {
            Topic topic = this.topicService.findById(id);
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            List<Post> posts = postService.findByTopic(id);
            TopicPosts topicPosts = new TopicPosts(topic, posts, user);
            BookClub bookClub = bookClubService.findById(topic.BookClubId);
            ViewBag.bookClubOwnerId = bookClub.BookHiveApplicationUserId;
            return View(topicPosts);
        }

        public IActionResult GetAddTopicForm(Guid id)
        {
            ViewBag.bookClubId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addNewTopic(Guid bookClubId, string title)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            topicService.save(title, user.Id, bookClubId);
            return RedirectToRoute("default", new { controller = "BookClub", action = "Details", id = bookClubId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addNewPostToTopic(Guid topicId, Guid bookClubId, string content)
        {
            BookHiveApplicationUser user = await userManager.GetUserAsync(HttpContext.User);
            postService.save(content, user.Id, topicId);
            return RedirectToRoute("default", new { controller = "Topic", action = "GetTopicById", id = topicId });

        }

        public IActionResult deletePostFromTopic(Guid id)
        {
            Topic topic = postService.findById(id).Topic;
            postService.deleteById(id);
            return RedirectToRoute("default", new { controller = "Topic", action = "GetTopicById", id = topic.Id });

        }
    }
}
