﻿namespace ARAMLadder.Models.LolModels
{
    public class ProfileIconDto
    {
        public int id { get; set; }
        public Image image { get; set; }
    }

    public class Image
    {
        public string full { get; set; }
        public string sprite { get; set; }
        public string group { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
    }

}
