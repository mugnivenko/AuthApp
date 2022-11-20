using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AuthApp.Models;
using AuthApp.Data;
using Microsoft.EntityFrameworkCore;
using AuthApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthApp.Controllers;

public class HomeController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly ILogger<HomeController> _logger;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;

  public HomeController(
      ApplicationDbContext context,
      ILogger<HomeController> logger,
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager)
  {
    _logger = logger;
    _context = context;
    _userManager = userManager;
    _signInManager = signInManager;
  }

  public async Task<IActionResult> Index()
  {
    if (CurrentUserBlocked())
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index");
    }
    var users = await _context.Users.ToListAsync();
    return View(users);
  }

  [HttpPost]
  public async Task<IActionResult> Index(string[] usersId, string action)
  {
    if (CurrentUserBlocked())
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index");
    }
    await PerformAction(action, usersId);
    return RedirectToAction("Index");
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }

  private ApplicationUser? GetCurrentUser(string? currentUserId)
  {
    var currentUsers = _context.Users.Where(user => user.Id == currentUserId).ToArray();
    if (currentUsers.Length == 0)
    {
      return null;
    }
    return currentUsers.First();
  }

  private bool CurrentUserBlocked()
  {
    var currentUserId = GetCurrentUserId();
    if (currentUserId == null)
    {
      return false;
    }
    var currentUser = GetCurrentUser(currentUserId);
    if (currentUser == null)
    {
      return true;
    }
    return currentUser.Blocked;
  }

  private async Task<IActionResult> PerformAction(string action, string[] usersId) =>
    action switch
    {
      "Block" => await BlockUsers(usersId),
      "Unblock" => await UnblockUsers(usersId),
      "Delete" => await DeleteUsers(usersId),
      _ => throw new ArgumentException("Invalid string value for action", nameof(action)),
    };

  private async Task UpdateUsersBlockedProperty(string[] usersId, bool blocked)
  {
    var selectedUsers = await _context.Users.Where(user => usersId.Contains(user.Id)).ToListAsync();
    foreach (var user in selectedUsers)
    {
      user.Blocked = blocked;
    }
    _context.SaveChanges();
  }

  private string? GetCurrentUserId()
  {
    return User.FindFirstValue(ClaimTypes.NameIdentifier);
  }

  private async Task LogoutUserIfSelected(string[] usersId)
  {
    var currentUserId = GetCurrentUserId();
    if (usersId.Contains(currentUserId))
    {
      await _signInManager.SignOutAsync();
    }
  }
  private async Task<IActionResult> BlockUsers(string[] usersId)
  {
    await LogoutUserIfSelected(usersId);
    await UpdateUsersBlockedProperty(usersId, true);
    return RedirectToAction("Index");
  }

  private async Task<IActionResult> UnblockUsers(string[] usersId)
  {
    await UpdateUsersBlockedProperty(usersId, false);
    return RedirectToAction("Index");
  }

  private async Task<IActionResult> DeleteUsers(string[] usersId)
  {
    await LogoutUserIfSelected(usersId);
    await _context.Users.Where(user => usersId.Contains(user.Id)).ExecuteDeleteAsync();
    return RedirectToAction("Index");
  }
}
