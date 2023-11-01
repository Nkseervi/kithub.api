using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace kithub.api.Areas.Identity.Data;

// Add profile data for application users by adding properties to the KithubUser class
public class KithubUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public DateTime CreateDate { get; set; }
}

