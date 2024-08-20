﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWP391.E.BL5.G3.Authorization;
using SWP391.E.BL5.G3.DAO_Context;
using SWP391.E.BL5.G3.Models;

namespace SWP391.E.BL5.G3.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly traveltestContext _traveltestContext;
        private readonly Cloudinary _cloudinary;

        public AdminController(traveltestContext traveltestContext, IOptions<CloudinarySettings> cloudinarySettings)
        {
            _traveltestContext = traveltestContext;
            var cloudinarySettingsValue = cloudinarySettings.Value;
            var account = new Account(
                cloudinarySettingsValue.CloudName,
                cloudinarySettingsValue.ApiKey,
                cloudinarySettingsValue.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Enum.RoleEnum.Admin)]
        // GET: TourGuide/CreateTourGuide
        public IActionResult CreateTourGuide()
        {
            return View();
        }

        // POST: TourGuide/CreateTourGuide
        [HttpPost]
        [Authorize(Enum.RoleEnum.Admin)]
        public async Task<IActionResult> CreateTourGuide([Bind("FirstName,LastName,PhoneNumber,Email,Description")] TourGuide tourGuide, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                tourGuide.FirstName = tourGuide.FirstName?.Trim();
                tourGuide.LastName = tourGuide.LastName?.Trim();
                tourGuide.PhoneNumber = tourGuide.PhoneNumber?.Trim();
                tourGuide.Email = tourGuide.Email?.Trim();
                tourGuide.Description = tourGuide.Description?.Trim();

                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(stream);
                        stream.Position = 0;

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(imageFile.FileName, stream)
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        tourGuide.Image = uploadResult.SecureUrl.ToString();
                    }

                    _traveltestContext.Add(tourGuide);
                    await _traveltestContext.SaveChangesAsync();
                }

                return RedirectToAction(nameof(TourGuideManagement));
            }

            return View(tourGuide);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EditTourGuide(int id)
        {
            var tourGuide = await _traveltestContext.TourGuides.FindAsync(id);
            if (tourGuide == null)
            {
                return NotFound();
            }

            return View(tourGuide);
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> EditTourGuide([Bind("TourGuideId,FirstName,LastName,PhoneNumber,Email,Description,Image,Rate")] TourGuide tourGuide, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingTourGuide = await _traveltestContext.TourGuides.FindAsync(tourGuide.TourGuideId);

                if (existingTourGuide == null)
                {
                    return NotFound();
                }

                existingTourGuide.FirstName = tourGuide.FirstName.Trim();
                existingTourGuide.LastName = tourGuide.LastName.Trim();
                existingTourGuide.PhoneNumber = tourGuide.PhoneNumber.Trim();
                existingTourGuide.Email = tourGuide.Email.Trim();
                existingTourGuide.Description = tourGuide.Description.Trim();

                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(stream);
                        stream.Position = 0;

                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(imageFile.FileName, stream)
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        existingTourGuide.Image = uploadResult.SecureUrl.ToString();
                    }
                }
                else
                {
                    existingTourGuide.Image = tourGuide.Image;
                }

                _traveltestContext.Update(existingTourGuide);
                await _traveltestContext.SaveChangesAsync();

                return RedirectToAction(nameof(TourGuideManagement));
            }

            return View(tourGuide);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TourGuideManagement(string searchQuery)
        {
            var query = _traveltestContext.TourGuides.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(tg =>
                    tg.Email.Contains(searchQuery) ||
                    tg.FirstName.Contains(searchQuery) ||
                    tg.LastName.Contains(searchQuery) ||
                    (tg.FirstName + " " + tg.LastName).Contains(searchQuery) ||
                    tg.PhoneNumber.Contains(searchQuery));
            }

            var tourGuides = await query.ToListAsync();

            ViewData["SearchQuery"] = searchQuery;
            return View(tourGuides);
        }

        public IActionResult ListRegisterTravelAgent()
        {
            DAO dal = new DAO();
            List<User> listuserregistertravelagent = dal.GetListUserRegisterTravelAgent();
            ViewBag.ListUserTravelAgent = listuserregistertravelagent;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> FeedbackManagement(string searchQuery, int page = 1, int pageSize = 10)
        {
            var query = _traveltestContext.Feedbacks
            .Include(f => f.User) // Include the User information
            .Where(f => !f.ParentId.HasValue); // Filter out feedbacks with a ParentId

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(f => f.Content.Contains(searchQuery) ||
                                          f.User.FirstName.Contains(searchQuery) ||
                                          f.User.LastName.Contains(searchQuery)); // Apply search filter
            }

            var totalItems = await query.CountAsync(); // Total items before pagination

            var feedbacks = await query
                .OrderByDescending(f => f.CreatedDate) // Order by creation date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Apply pagination

            var pagedResult = new PagedResult<Feedback>
            {
                Items = feedbacks,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };

            ViewData["SearchQuery"] = searchQuery;

            return View(pagedResult);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ReplyFeedback(int id)
        {
            var feedback = _traveltestContext.Feedbacks.FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            var user = _traveltestContext.Users.FirstOrDefault(u => u.UserId == feedback.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ReplyFeedbackViewModel
            {
                Feedback = feedback,
                UserAvatar = user.Image,
                UserFirstName = user.FirstName,
                UserLastName = user.LastName
            };

            return View(viewModel);
        }

        public IActionResult RequestAccept(int id, string email)
        {
            DAO dal = new DAO();
            string fromEmail = "duydqhe163434@fpt.edu.vn";
            string toEmail = email;
            string subject = "Hello " + email;

            string body = "Tài Khoản Của Bạn Đã Đăng Ký TravelAgent Thành Công";
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "duydqhe163434@fpt.edu.vn";
            string smtpPassword = "htay mxgi flsx dxde";
            bool result = SendEmail.theSendEmailRegisterTravelAgent(fromEmail, toEmail, subject, body, smtpServer, smtpPort, smtpUsername, smtpPassword);
            string stt = "Accept";
            dal.AccessRegisterTravelAgent(id, stt);
            return RedirectToAction("ListRegisterTravelAgent", "Admin");
        }
        public IActionResult RequestUnaccept(int id, string email)
        {
            DAO dal = new DAO();
            string fromEmail = "duydqhe163434@fpt.edu.vn";
            string toEmail = email;
            string subject = "Hello " + email;

            string body = "Vì Một Số Lý Do Nào Đó Từ Phía Của Chúng Tôi Không Thể Cung Cấp Dịch Vụ Travelagent Cho Bạn Được Nữa Mọi Chi Tiết Xin Vui Lòng Liên Hệ Admin ";
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "duydqhe163434@fpt.edu.vn";
            string smtpPassword = "htay mxgi flsx dxde";
            bool result = SendEmail.theSendEmailRegisterTravelAgent(fromEmail, toEmail, subject, body, smtpServer, smtpPort, smtpUsername, smtpPassword);
            string stt = "Unaccept";
            dal.AccessRegisterTravelAgent(id, stt);
            return RedirectToAction("ListRegisterTravelAgent", "Admin");
        }

    }

    public class CloudinarySettings
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / PageSize);
    }

    public class ReplyFeedbackViewModel
    {
        public Feedback Feedback { get; set; }
        public string UserAvatar { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string ReplyContent { get; set; }
    }
}


