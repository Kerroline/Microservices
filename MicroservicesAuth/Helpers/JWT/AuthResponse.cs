using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSAuth.Models
{
    public class AuthResponse
    {
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public AuthResponse(string jwt, string refresh)
        {
            this.JwtToken = jwt;
            this.RefreshToken = refresh;
        }
    }
}
