using Microsoft.Owin;

namespace Elmo.Viewer
{
    public class ElmoViewerOptions
    {
        public PathString Path { get; set; } = new PathString("/elmo");
        public bool AllowRemoteAccess { get; set; }
    }
}
