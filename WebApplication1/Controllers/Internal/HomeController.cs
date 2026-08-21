
namespace WebApplication1.Controllers.Internal;

public class HomeController : BaseInternalController
{
    [HttpGet]
    public string Index()
    {
        return "hello internal api service";
    }
}
