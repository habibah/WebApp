using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace WebApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        // GET: Notes
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var userNotes =
                await
                    _db.Users.Where(u => u.Id == userId)
                        .Include(u => u.Notes)
                        .SelectMany(u => u.Notes)
                        .ToListAsync();

            return View(userNotes);
        }

        // GET: Notes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId();
            ICollection<Note> userNotes = (
                await _db.Users.Where(u => u.Id == userId)
                .Include(u => u.Notes).Select(u => u.Notes)
                .FirstOrDefaultAsync());

            if (userNotes == null)
            {
                return HttpNotFound();
            }

            Note note = userNotes.FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // GET: Notes/Create
        public ActionResult Create()
        {
            return View();
        }
             

        // GET: Notes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!await NoteBelongToUser(User.Identity.GetUserId(), noteId: id.Value))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Note note = await _db.Notes.FindAsync(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: Notes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Text,CreatedAt")] Note note)
        {
            if (ModelState.IsValid && await NoteBelongToUser(User.Identity.GetUserId(), note.Id))
            {
                _db.Entry(note).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(note);
        }

        // GET: Notes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!await NoteBelongToUser(User.Identity.GetUserId(), noteId: id.Value))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Note note = await _db.Notes.FindAsync(id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!await NoteBelongToUser(User.Identity.GetUserId(), noteId: id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Note note = await _db.Notes.FindAsync(id);
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        private async Task<bool> NoteBelongToUser(string userId, int noteId)
        {
            return await _db.Users.Where(u => u.Id == userId).Where(u => u.Notes.Any(n => n.Id == noteId)).AnyAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
