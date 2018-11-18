using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class LoginGameSpell
    {
        public int Id { get; set; }
        public LoginGame LoginGame { get; set; }
        public int LoginGameId { get; set; }
        public Spell Spell { get; set; }
        public int SpellId { get; set; }
        public int Position { get; set; }
    }
}
