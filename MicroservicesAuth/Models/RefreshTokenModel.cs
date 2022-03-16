using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSAuth.Models
{
    public class RefreshTokenModel
    {
        [JsonIgnore]
        [Key]
        public int Id { get; set; }

        [JsonIgnore]
        public string Token { get; set; }

        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }

        public string ByUserId { get; set; }

        [ForeignKey("ByUserId")]
        public virtual CustomUserModel User { get; set; }
    }
}
