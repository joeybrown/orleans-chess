using System.Threading.Tasks;

public interface IHello : Orleans.IGrainWithGuidKey
{
    Task<string> SayHello(string msg);
}