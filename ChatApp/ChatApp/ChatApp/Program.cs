using ChatApp.Components;
using ChatApp.Domain.Interfaces;
using ChatApp.Domain.Repositories;
using ChatApp.Hubs;
using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────  Data / Identity  ────────────────────────────

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ChatAppContext>(opts =>
    opts.UseSqlite(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ChatAppContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddSignalR(); 

builder.Services.AddRazorPages();   // Identity UI (harmless if UI not added)

// ─────────────────────────────  Blazor / MVC  ──────────────────────────────

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ─────────────────────────────────  CORS  ───────────────────────────────────

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", pb =>
        pb.AllowAnyHeader()
          .AllowAnyMethod()
          .AllowAnyOrigin());
});

// ───────────────────────────────  Repositories  ────────────────────────────

builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatRoomMembershipRepository, ChatRoomMembershipRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// ──────────────────────────────  Build / Pipeline  ─────────────────────────

builder.Services.AddControllers();   // add this line

// Register an HttpClient whose BaseAddress = https://{host}/
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapControllers();               // add this after app.MapRazorPages();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();      // ⬅️  **added back – after authN/authZ, before endpoints**

app.MapRazorPages();       // needed only if you scaffold/enable Identity UI

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ChatApp.Client._Imports).Assembly);

app.MapHub<ChatHub>("/chathub");

app.Run();
