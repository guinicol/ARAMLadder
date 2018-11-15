using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class Game
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long GameCreation { get; set; }
        public int GameDuration { get; set; }
        public int KillAlly { get; set; }
        public int KillEnnemy { get; set; }

        public IList<LoginGame> LoginGames {get; set; }
    }
}
