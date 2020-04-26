using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class AramIdentityUser : IdentityUser
    {
        public string riotId { get; set; }
        public string riotName { get; set; }
    }
}
