using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookListMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _db;   // bcoz we want to acccess the DB
        [BindProperty]
        public Book Book { get; set; }

        public BooksController(ApplicationDbContext db)
        {
            // To get access to the DB using Dependency Injection
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upset(int? id)
        {
            Book = new Book();      // initialize the Book Object
            if (id == null)
            {
                // create
                return View(Book);
            }

            // Update
            Book = await _db.Books.FirstOrDefaultAsync(u => u.Id == id);
            if (Book == null)
            {
                return NotFound();
            }

            return View(Book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]      // to use in-built security to prevent attack
        public async Task<IActionResult> Upset()    // NB: it is meant to be Upsert
        {
            if (ModelState.IsValid)
            {
                if (Book.Id == 0)
                {
                    // create 
                    await _db.Books.AddAsync(Book);
                }
                else
                {
                    // update 
                    _db.Books.Update(Book);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");    // NB: while Razor use "RedirectToPage", MVC use "RedirectToAction"
            }

            return View(Book);
        }

        #region API Calls 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Books.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // get the book with the ID that you want to delete
            var bookFromDb = await _db.Books.FirstOrDefaultAsync(u => u.Id == id);
            if (bookFromDb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            _db.Books.Remove(bookFromDb);
            await _db.SaveChangesAsync();
            return Json(new { sucess = true, message = "Delete Successfully" });
        }
        #endregion
    }
}