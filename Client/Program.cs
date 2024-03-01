using Client.Services.Network;

internal class Program
{
    private static void Main(string[] args)
    {
        //ChatClient cl = new(10000, "26.98.33.97", "pidr228@gmail.com", "pidr228", NetworkInterfaceUtility.GetRadminVPNIPAddress().ToString());
        ChatClient cl = new(10000, "127.0.0.1", "test1@gmail.com", "test1228", "127.0.0.1");
        //ChatClient cl = new(10000, "127.0.0.1", "127.0.0.1", "test", "test228", "GOD", "test@gmail.com", DateTime.Now);
        cl.Init();
        Thread.Sleep(1000);
        //cl.CloseConnection();
        //if (!cl.GetFile("test.txt"))
        //{
        //    Thread.Sleep(1500);
        //    cl.GetFile("test.txt");
        //}
    }
}