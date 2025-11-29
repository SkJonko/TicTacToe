var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TicToe_Infinite>("tictoe-infinite");

builder.Build().Run();
