using Microsoft.Owin;

namespace Elmo
{
    public class ElmoOptions
    {
        public PathString Path { get; set; } = new PathString("/elmo");
        public bool AllowRemoteAccess { get; set; }
    }
}
