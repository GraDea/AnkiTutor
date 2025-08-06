// Program.cs
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

// Configuration: set BOT_TOKEN and GCP_PROJECT in appsettings or env vars
// e.g. DOTNET_ENVIRONMENT=Development, BOT_TOKEN=..., GCP_PROJECT=your-gcp-project

// Register Firestore
builder.Services.AddSingleton(_ =>
{
    var projectId = builder.Configuration["GCP_PROJECT"];
    return FirestoreDb.Create(projectId);
});

// Register TelegramBotClient
builder.Services.AddSingleton(_ =>
    new TelegramBotClient(builder.Configuration["BOT_TOKEN"]));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/bot", async (
    [FromServices] TelegramBotClient botClient,
    [FromServices] FirestoreDb db,
    [FromBody] Update update
) =>
{
    var msg = update.Message;
    if (msg?.Text != null && msg.Text.StartsWith("/add "))
    {
        var phrase = msg.Text["/add ".Length..].Trim();
        if (!string.IsNullOrEmpty(phrase))
        {
            var col = db.Collection("phrases");
            await col.AddAsync(new { text = phrase, addedAt = Timestamp.GetCurrentTimestamp() });
            await botClient.SendMessage(msg.Chat.Id, "✅ Phrase added.");
        }
        else
        {
            await botClient.SendMessage(msg.Chat.Id, "❗️Usage: /add your phrase");
        }
    }
    return Results.Ok();
});

app.Run();