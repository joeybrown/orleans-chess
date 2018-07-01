using System.Threading.Tasks;

class HelloGrain : Orleans.Grain, IHello
{
    public Task<string> SayHello(string msg)
    {
        return Task.FromResult(string.Format("You said {0}, I say: Hello!", msg));
    }
}