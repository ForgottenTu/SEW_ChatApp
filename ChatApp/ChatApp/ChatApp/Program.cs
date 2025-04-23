using ChatApp.Components;
using ChatApp.Domain.Interfaces;
using ChatApp.Domain.Repositories;
using ChatApp.Hubs;
using ChatApp.Model.Context;
using ChatApp.Model.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/*
 * Program.cs – ChatApp (Blazor Web App, .NET 8)
 * -----------------------------------------------------------------------------
 * Complete startup file with authentication‑aware SignalR configuration.
 */

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────  Data / Identity  ───────────────────────────────

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ChatAppContext>(opts =>
    opts.UseSqlite(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ChatAppContext>()
    .AddDefaultTokenProviders();

// Explicitly register authN / authZ middleware
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();              // needed to copy cookies to server‑side SignalR clients
builder.Services.AddSignalR();

// Razor Pages (Identity UI) & controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Blazor (interactive server + optional WebAssembly)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ─────────────────────────────────────  CORS  ─────────────────────────────────────

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", pb =>
        pb.AllowAnyHeader()
          .AllowAnyMethod()
          .AllowAnyOrigin());
});

// ───────────────────────────────────  Repositories  ─────────────────────────────────

builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatRoomMembershipRepository, ChatRoomMembershipRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// HttpClient whose BaseAddress = https://{host}/ (used by server‑side components)
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

// ──────────────────────────────────  Build / Pipeline  ─────────────────────────────

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");
app.UseAuthentication();                           // must appear before MapHub
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();                               // Identity UI endpoints
app.MapControllers();

app.MapHub<ChatHub>("/chathub").RequireAuthorization().DisableAntiforgery();   // secure SignalR hub

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ChatApp.Client._Imports).Assembly);

app.Run();
