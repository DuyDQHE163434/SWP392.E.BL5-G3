﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.E.BL5.G3.Models;

namespace SWP391.E.BL5.G3.Controllers
{
    public class ToursController : Controller
    {
        private readonly traveltestContext _context;

        public ToursController(traveltestContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ListTour(string searchString)
        {
            var tours = from t in _context.Tours
                        select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                tours = tours.Where(s => s.Name.Contains(searchString));
            }

            return View(await tours.ToListAsync());
        }

        public async Task<IActionResult> TourDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        public IActionResult CreateTour()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTour([Bind("Name,Description,Price")] Tour tour, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    tour.Image = uniqueFileName;
                }

                tour.CreateDate = DateTime.Now;
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListTour));
            }
            return View(tour);
        }

        // Edit Tour
        public async Task<IActionResult> EditTour(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTour(int id, [Bind("TourId,Name,Image,Description,Price,Duration,AirPlane,Rating,Itinerary,Inclusions,Exclusions,GroupSize,Guide")] Tour tour, IFormFile image)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null && image.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        tour.Image = uniqueFileName;
                    }

                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListTour));
            }
            return View(tour);
        }


        // Delete Tour
        public async Task<IActionResult> DeleteTour(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        [HttpPost, ActionName("DeleteTour")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTourConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListTour));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }
    }
}