using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.WindowsPresentation.Track
{
    public class Track
    {
        public readonly IList<IList<TrackRecord>> Segments = new List<IList<TrackRecord>>();
    }
}
