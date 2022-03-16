using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Models
{
    public class TokenClaims
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public TokenClaims(string id, string username, IEnumerable<string> roles)
        {
            this.Id = id;
            this.Username = username;
            this.Roles = roles;
        }
    }
}
