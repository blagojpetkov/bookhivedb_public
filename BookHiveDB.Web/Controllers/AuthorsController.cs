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
using BookHiveDB.Domain.DTO;
using ExcelDataReader;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace BookHiveDB.Web.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorService authorService;

        public AuthorsController(IAuthorService authorService)
        {
            this.authorService = authorService;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(authorService.findAll());
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var author = authorService.findById(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,surname,age,biography,Id")] Author author)
        {
            if (ModelState.IsValid)
            {
                author.Id = Guid.NewGuid();
                authorService.Insert(author);
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }


        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = authorService.findById(id.Value);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("name,surname,age,biography,Id")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    authorService.Update(author);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(author);
        }

     
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = authorService.findById(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            authorService.deleteById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(Guid id)
        {
            return authorService.findById(id) != null;
        }


        public ActionResult ImportAuthorsView()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportAuthors(IFormFile file)
        {
            string pathToUpload = $"{Directory.GetCurrentDirectory()}\\files\\{file.FileName}";

            using (FileStream fileStream = System.IO.File.Create(pathToUpload))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }

            List<UserExcelDto> authors = getAllAuthorsFromFile(file.FileName);

            foreach (var author in authors)
            {
                authorService.add(author.name, author.surname, author.age, author.biography);
            }
            return RedirectToAction("Index"); 
        }

        private List<UserExcelDto> getAllAuthorsFromFile(string fileName)
        {
            List<UserExcelDto> authors = new List<UserExcelDto>();
            string filePath = $"{Directory.GetCurrentDirectory()}\\files\\{fileName}";
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        authors.Add(new UserExcelDto
                        {
                            name = reader.GetValue(0).ToString(),
                            surname = reader.GetValue(1).ToString(),
                            age = Convert.ToInt32(reader.GetValue(2)),
                            biography = reader.GetValue(3).ToString()
                        });
                    }
                }
            }
            return authors;
        }
    }
}
