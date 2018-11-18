using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class LoginGameItem
    {
        public int Id { get; set; }
        public LoginGame LoginGame { get; set; }
        public int LoginGameId { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
    }
}
