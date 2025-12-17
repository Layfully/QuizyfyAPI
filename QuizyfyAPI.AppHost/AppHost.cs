IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var sql = builder.AddSqlServer("sql-server")
    .WithDataVolume() // Persist data between restarts
    .WithLifetime(ContainerLifetime.Persistent) // Keep container running when debugging stops
    .AddDatabase("QuizyfyDB"); // The database name

builder.AddProject<Projects.QuizyfyAPI>("api")
    .WithReference(sql) // Automatically injects ConnectionStrings__QuizyfyDB
    .WithReference(redis)
    .WaitFor(sql)       // Don't start API until DB is ready
    // Map the Aspire ConnectionString to your custom AppOptions format
    .WithEnvironment("AppOptions__ConnectionString", sql.Resource.ConnectionStringExpression)
    .WithEnvironment("JwtOptions__Secret", "This_Is_A_Super_Strong_Secret_Key_For_Development_32_Chars!"); 
builder.Build().Run();
