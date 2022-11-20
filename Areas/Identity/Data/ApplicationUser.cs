using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AuthApp.Areas.Identity.Data;

enum Status
{

}

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
  public DateTime LastLoginTime { get; set; } = DateTime.Now;

  public DateTime RegisterTime { get; set; } = DateTime.Now;

  [Required]
  public bool Blocked { get; set; }
}

