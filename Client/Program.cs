using Client.Services.Network;

internal class Program
{
    private static void Main(string[] args)
    {
        ChatClient cl = new(10000, "127.0.0.1", "pidr228@gmail.com", "pidr228", "127.0.0.1");
        //ChatClient cl = new(10000, "127.0.0.1", "127.0.0.1", "pidr", "pidr228", "PIDR", "pidr228@gmail.com", DateTime.Now);
        cl.Init();
        Thread.Sleep(1000);
        if (!cl.GetFile("test.txt"))
        {
            Thread.Sleep(1500);
            cl.GetFile("test.txt");
        }
    }
}