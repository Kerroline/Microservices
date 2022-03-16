using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Models
{
    public class UserWithRoles
    {
        public CustomUserModel User { get; set; }

        public HashSet<string> Roles { get; set; }


        public UserWithRoles(CustomUserModel user, IEnumerable<string> roles)
        {
            this.User = user;
            this.Roles = new();
            SetRoles(roles);
        }

        private void SetRoles(IEnumerable<string> roles)
        {
            foreach(string role in roles)
            {
                if (!Roles.Contains(role))
                {
                    Roles.Add(role);
                }
            }
        }
    }
}
