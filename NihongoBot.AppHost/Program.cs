IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("Postgres")
						.WithDataVolume(isReadOnly: false)
						.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> postgresdb = postgres.AddDatabase("NihongoBotDB");

IResourceBuilder<ProjectResource> server = builder.AddProject<Projects.NihongoBot_Server>("NihongoBotServer")
							.WithReference(postgresdb)
							.WaitFor(postgresdb);

IResourceBuilder<ProjectResource> bot = builder.AddProject<Projects.NihongoBot>("NihongoBot")
							.WithReference(postgresdb)
							.WaitFor(postgresdb);

builder.Build().Run();
