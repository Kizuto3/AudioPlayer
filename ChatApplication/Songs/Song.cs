using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication
{
    public class Song : BaseViewModel
    {
        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string FullPath { get; set; } 

        public Song(string title, string artist, string album, string fullpath)
        {
            this.Title = title;
            this.Artist = artist;
            this.Album = album;
            this.FullPath = fullpath;
        }

    }
}
