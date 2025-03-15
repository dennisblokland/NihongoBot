using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var postgres = builder.AddPostgres("Postgres")
						.WithDataVolume(isReadOnly: false)
						.WithPgAdmin();

var postgresdb = postgres.AddDatabase("NihongoBotDB");

var server = builder.AddProject<Projects.NihongoBot_Server>("NihongoBotServer")
							.WithReference(postgresdb);

var bot = builder.AddProject<Projects.NihongoBot>("NihongoBot")
							.WithReference(postgresdb);

builder.Build().Run();
