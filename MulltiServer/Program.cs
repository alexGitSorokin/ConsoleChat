using MulltiServer;

Server server = new Server();
Console.Title = "Server";
server.SetupServer();
Console.ReadLine();
server.CloseAllSockets();