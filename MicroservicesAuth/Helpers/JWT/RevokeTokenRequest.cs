using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Models
{
    public class RevokeTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
