using Citiz.Content;
using Citiz.ContentWorker.Monitoring;

// Citiz.ContentWorker — stage one of the design's update pipeline: detect that an official source
// changed. Everything after detection (extraction, diff, editorial approval, publication) is a human
// process today, by design; the worker's job is to make sure nobody has to remember to look.

var builder = Host.CreateApplicationBuilder(args);

var contentRoot = builder.Configuration["Content:Root"]
    ?? FileContentStore.LocateContentRoot(builder.Environment.ContentRootPath)
    ?? throw new InvalidOperationException("Set Content:Root to the content folder, or run from inside the repository.");

builder.Services.AddSingleton<IContentStore>(new FileContentStore(contentRoot));
builder.Services.AddSingleton<ContentRepository>();
builder.Services.Configure<MonitoringOptions>(builder.Configuration.GetSection("Monitoring"));
builder.Services.AddHttpClient<SourceMonitor>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Citiz-ContentWorker/0.3 (+https://github.com/peopleworks/Citiz)");
});
builder.Services.AddHostedService<SourceMonitorService>();

await builder.Build().RunAsync();
