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

namespace user_groups.Controllers
{
    
    [Authorize]
    public class GroupItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        //@if (SignInManager.IsSignedIn(User))  This could be usefull for checking if someone is logged in before any action in a method is called

        public GroupItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        //Check if the current user is the host of the group
        public async Task<bool> checkHost(GroupItem group)
        {
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            if (group.Host == userId)
            {
                return true;
            }
            return false;
        }

        // GET: GroupItems
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            var groupsQueryable = from groupUserLink in _context.GroupUserLink
                                  join groupItem in _context.GroupItem on groupUserLink.GroupItemID equals groupItem.GroupItemID
                                  where groupUserLink.ApplicationUserID == userId
                                  where groupUserLink.Active == true
                                  select groupItem;

            var groups = groupsQueryable.ToList();
            return View(groups);
        }

        // GET: GroupItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupItem = await _context.GroupItem.SingleOrDefaultAsync(m => m.GroupItemID == id);
            if (groupItem == null)
            {
                return NotFound();
            }

            return View(groupItem);
        }

        // GET: GroupItems/Create
        public IActionResult Create()
        {
            return View();
        }

        public async Task<GroupUserLink> GroupUserLinkCreate(GroupItem group)
        {
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            GroupUserLink link = new GroupUserLink();
            link.Active = true;
            link.GroupItemID = group.GroupItemID;
            link.ApplicationUserID = userId;
            _context.Add(link);

            return (link);
        }

        // POST: GroupItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupItemID,GroupName,MemberCount,Host")] GroupItem groupItem)
        {
            var user = await GetCurrentUserAsync();
            var userId = user?.Id;
            groupItem.Host = userId;
            _context.Add(groupItem);
            await _context.SaveChangesAsync();

            GroupUserLink link = await GroupUserLinkCreate(groupItem);
            _context.Add(link);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: GroupItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupItem = await _context.GroupItem.SingleOrDefaultAsync(m => m.GroupItemID == id);
            if (groupItem == null)
            {
                return NotFound();
            }
            if (await checkHost(groupItem))
                return View(groupItem);
            else
                return Unauthorized();
        }

        // POST: GroupItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GroupItemID,GroupName,Host,MemberCount")] GroupItem groupItem)
        {
            if (await checkHost(groupItem))
            {
                if (id != groupItem.GroupItemID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(groupItem);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!GroupItemExists(groupItem.GroupItemID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Index");
                }
                return View(groupItem);
            }
            else
                return Unauthorized();
        }

        // GET: GroupItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupItem = await _context.GroupItem.SingleOrDefaultAsync(m => m.GroupItemID == id);
            if (groupItem == null)
            {
                return NotFound();
            }

            if (await checkHost(groupItem))
                return View(groupItem);
            else
                return Unauthorized();
        }

        // POST: GroupItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupItem = await _context.GroupItem.SingleOrDefaultAsync(m => m.GroupItemID == id);

            if (await checkHost(groupItem))
            {
                var user = await GetCurrentUserAsync();
                var userId = user?.Id;
                
                //Remove all related group User links
                var links = _context.GroupUserLink.Where(b => b.GroupItemID == groupItem.GroupItemID).ToList();
                IEnumerable<GroupUserLink> groupUserLinks = links;

                foreach (GroupUserLink link in groupUserLinks)
                {
                    _context.GroupUserLink.Remove(link);
                }


                _context.GroupItem.Remove(groupItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
                return Unauthorized();
        }

        private bool GroupItemExists(int id)
        {
            return _context.GroupItem.Any(e => e.GroupItemID == id);
        }



        // GET: Request/Create
        public async Task<IActionResult> CreateRequest(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupItem = await _context.GroupItem.SingleOrDefaultAsync(m => m.GroupItemID == id);
            if (groupItem == null)
            {
                return NotFound();
            }
            if (await checkHost(groupItem))
            {
                GroupUserLink link = new GroupUserLink();
                link.GroupItemID = groupItem.GroupItemID;
                link.Active = false;
                return View(link);
            }
            else
                return Unauthorized();
        }

        public bool validRequest(GroupUserLink request)
        {
            string user = request.ApplicationUserID;
            var requestQueryable = from groupUserLink in _context.GroupUserLink
                                   join groupItem in _context.GroupItem on groupUserLink.GroupItemID equals groupItem.GroupItemID
                                   where groupUserLink.ApplicationUserID == user
                                   select groupUserLink;
            IEnumerable<GroupUserLink> existingLinks = requestQueryable.ToList();
            foreach(GroupUserLink link in existingLinks)
            {
                if (link.GroupItemID == request.GroupItemID)
                    return false;
            }
            return true;
        }

        public string findRecipient(string email)
        {
           List<ApplicationUser> users =  _userManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                Console.WriteLine(user.Email);
                if (user.Email == email) return user.Id;
            }

            return "Not Found";
        }

        // POST: GroupUserLinks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest([Bind("GroupUserLinkID,GroupItemID,ApplicationUserID")] GroupUserLink groupUserLink)
        {
            if (ModelState.IsValid && validRequest(groupUserLink))
            {
                var user = await GetCurrentUserAsync();
                string emailAddress = groupUserLink.ApplicationUserID;

                string recipient = findRecipient(emailAddress);
                if (recipient == "Not Found") return RedirectToAction("Index");
                groupUserLink.ApplicationUserID = recipient;
                
                _context.Add(groupUserLink);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(groupUserLink);
        }
    }
}
