using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Models
{
    public class HomeViewModel
    {
        public List<Game> Games { get; set; }
        public int nbWin { get; set; }
        public int nbLose { get; set; }

    }
}
