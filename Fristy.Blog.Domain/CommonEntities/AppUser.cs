using Microsoft.AspNetCore.Identity;

namespace Fristy.Blog.Domain
{
	public sealed class AppUser : IdentityUser
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }
	}
}
