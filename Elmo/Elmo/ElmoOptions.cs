using Microsoft.Owin;

namespace Elmo
{
    public class ElmoOptions
    {
        public PathString Path { get; set; }
        public bool AllowRemoteAccess { get; set; }
    }
}
