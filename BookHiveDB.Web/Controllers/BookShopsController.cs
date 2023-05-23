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

namespace BookHiveDB.Web.Controllers
{
    public class BookShopsController : Controller
    {
        private readonly IBookShopService bookshopService;

        public BookShopsController(IBookShopService bookshopService)
        {
            this.bookshopService = bookshopService;
        }


        public async Task<IActionResult> Index()
        {
            return View(bookshopService.GetAll());
        }

        public async Task<IActionResult> Search(string search)
        {
            if(search == null || search == "") return View("Index", bookshopService.GetAll());
            return View("Index", bookshopService.findAllByName(search));
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookShop = bookshopService.Get(id);
            if (bookShop == null)
            {
                return NotFound();
            }

            return View(bookShop);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("address,city,name,bookshopEmail,phoneNumber,websiteLink,latitude,longitude,grade,numGraders,Id")] BookShop bookShop)
        {
            if (ModelState.IsValid)
            {
                bookShop.Id = Guid.NewGuid();
                bookshopService.Insert(bookShop);
                return RedirectToAction(nameof(Index));
            }
            return View(bookShop);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookShop = bookshopService.Get(id);
            if (bookShop == null)
            {
                return NotFound();
            }
            return View(bookShop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("address,city,name,bookshopEmail,phoneNumber,websiteLink,latitude,longitude,grade,numGraders,Id")] BookShop bookShop)
        {
            if (id != bookShop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bookshopService.Update(bookShop);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookShopExists(bookShop.Id))
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
            return View(bookShop);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookShop = bookshopService.Get(id);
            if (bookShop == null)
            {
                return NotFound();
            }

            return View(bookShop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            bookshopService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Geolocation(Guid id)
        {
            ViewData["bookId"] = id;
            return View();
        }


        private bool BookShopExists(Guid id)
        {
            return bookshopService.Get(id) != null;
        }
    }
}
