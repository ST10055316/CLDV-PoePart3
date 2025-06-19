using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly EventEaseDBContext _context;
        private readonly IImageService _imageService;

        public EventController(EventEaseDBContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Events
        public async Task<IActionResult> Index(int? eventTypeId, int? venueId, DateTime? startDate, DateTime? endDate)
        {
            ViewData["EventTypeId"] = new SelectList(_context.EventType, "EventTypeId", "EventTypeName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");

            var query = _context.Events
                .Include(e => e.EventType)
                .Include(e => e.Venue)
                .AsQueryable();

            if (eventTypeId.HasValue)
            {
                query = query.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            if (venueId.HasValue)
            {
                query = query.Where(e => e.VenueId == venueId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EventDate <= endDate.Value);
            }

            return View(await query.ToListAsync());
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // ADDED: Include EventType
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var eventViewModel = new EventViewModel
            {
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName
                }).ToList(),
                // ADDED: Populate EventTypeList for the dropdown
                EventTypeList = _context.EventType.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.EventTypeName
                }).ToList(),
                EventDate = DateTime.Today
            };

            return View(eventViewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel eventViewModel)
        {
            // It's good practice to re-populate lists if ModelState is invalid
            if (!ModelState.IsValid)
            {
                eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName
                }).ToList();
                // ADDED: Re-populate EventTypeList if ModelState is invalid
                eventViewModel.EventTypeList = _context.EventType.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.EventTypeName
                }).ToList();
                return View(eventViewModel);
            }

            var @event = new Event
            {
                EventName = eventViewModel.EventName,
                EventDate = eventViewModel.EventDate,
                Description = eventViewModel.Description,
                VenueId = eventViewModel.VenueId,
                EventTypeId = eventViewModel.EventTypeId // ADDED: Map EventTypeId from ViewModel
            };

            if (eventViewModel.ImageFile != null)
            {
                @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var booking = new Booking
            {
                EventId = @event.EventId,
                VenueId = @event.VenueId,
                BookingDate = @event.EventDate
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            var eventViewModel = new EventViewModel
            {
                EventId = @event.EventId,
                EventName = @event.EventName,
                EventDate = @event.EventDate,
                Description = @event.Description,
                VenueId = @event.VenueId,
                EventTypeId = @event.EventTypeId, // ADDED: Populate existing EventTypeId
                ExistingImageUrl = @event.ImageUrl,
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName,
                    Selected = v.VenueId == @event.VenueId
                }).ToList(),
                // ADDED: Populate EventTypeList for the dropdown, pre-selecting the current one
                EventTypeList = _context.EventType.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.EventTypeName,
                    Selected = et.EventTypeId == @event.EventTypeId
                }).ToList()
            };

            return View(eventViewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel eventViewModel)
        {
            if (id != eventViewModel.EventId) return NotFound();

            if (!ModelState.IsValid)
            {
                // ADDED: Re-populate lists if ModelState is invalid
                eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName
                }).ToList();
                eventViewModel.EventTypeList = _context.EventType.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.EventTypeName
                }).ToList();
                return View(eventViewModel);
            }

            try
            {
                var @event = await _context.Events.FindAsync(id);
                if (@event == null) return NotFound(); // Should not happen if Edit(id) found it.

                var oldVenueId = @event.VenueId;
                var oldEventDate = @event.EventDate;

                @event.EventName = eventViewModel.EventName;
                @event.EventDate = eventViewModel.EventDate;
                @event.Description = eventViewModel.Description;
                @event.VenueId = eventViewModel.VenueId;
                @event.EventTypeId = eventViewModel.EventTypeId; // ADDED: Update EventTypeId

                if (eventViewModel.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("placeholder"))
                    {
                        await _imageService.DeleteImageAsync(@event.ImageUrl);
                    }
                    @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
                }

                _context.Update(@event);

                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.EventId == id && b.VenueId == oldVenueId && b.BookingDate == oldEventDate);

                if (booking != null)
                {
                    booking.VenueId = @event.VenueId;
                    booking.BookingDate = @event.EventDate;
                    _context.Update(booking);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(eventViewModel.EventId))
                    return NotFound();
                else
                    throw;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("duplicate") == true ||
                    ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    ModelState.AddModelError(string.Empty, "This venue is already booked for the specified date.");
                    // ADDED: Re-populate lists on error
                    eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
                    {
                        Value = v.VenueId.ToString(),
                        Text = v.VenueName
                    }).ToList();
                    eventViewModel.EventTypeList = _context.EventType.Select(et => new SelectListItem
                    {
                        Value = et.EventTypeId.ToString(),
                        Text = et.EventTypeName
                    }).ToList();
                    return View(eventViewModel);
                }
                throw;
            }
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // ADDED: Include EventType
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventToDelete = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventToDelete == null)
            {
                return NotFound();
            }

            if (eventToDelete.Bookings.Any())
            {
                TempData["ErrorMessage"] = $"Event \"{eventToDelete.EventName}\" cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event \"{eventToDelete.EventName}\" was successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}