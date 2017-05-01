using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using user_groups.Data;
using user_groups.Models;
using System.Security.Claims;
using user_groups.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace user_groups.Controllers.user_groups
{
    [Authorize]
    public class RequestController : Controller
    {
       
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;

            var requestQueryable = from groupUserLink in _context.GroupUserLink
                                   join groupItem in _context.GroupItem on groupUserLink.GroupItemID equals groupItem.GroupItemID
                                   where groupUserLink.ApplicationUserID == userId
                                   where groupUserLink.Active == false
                                   select groupUserLink;
            IEnumerable<GroupUserLink> requests = requestQueryable.ToList();

            if(requests.Count() == 0)
                return View(requests);
            if (requests.First().ApplicationUserID == userId)
                return View(requests);
            else return Unauthorized();
        }

        public async Task<IActionResult> Accept(int? id)
        {
            var link = await _context.GroupUserLink.SingleOrDefaultAsync(m => m.GroupUserLinkID == id);
            link.Active = true;
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            if (link.ApplicationUserID == userId)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else return Unauthorized();
        }

        public async Task<IActionResult> Reject(int? id)
        {
            var link = await _context.GroupUserLink.SingleOrDefaultAsync(m => m.GroupUserLinkID == id);
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            if (link.ApplicationUserID == userId)
            {
                _context.GroupUserLink.Remove(link);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else return Unauthorized();
        }
    }
 }

