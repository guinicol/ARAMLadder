using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class LoginGameRune
    {
        public int Id { get; set; }
        public LoginGame LoginGame { get; set; }
        public int LoginGameId { get; set; }
        public Rune Rune { get; set; }
        public int RuneId { get; set; }
        public int Position { get; set; }
    }
}
