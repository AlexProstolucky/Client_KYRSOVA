using Client.Services.Network;

internal class Program
{
    private static void Main(string[] args)
    {
        ChatClient cl = new(10000, "127.0.0.1", "Alex");
        cl.Init();

    }
}