using Azure;
using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shiko.ChatProvider.API.Data;
using Shiko.ChatProvider.API.Dtos;
using Shiko.ChatProvider.API.Models;

namespace Shiko.ChatProvider.API.Services;

public class ChatRoomService : IChatRoomService
{
    private readonly IConfiguration _config;
    private readonly string _acsEndpoint;
    private readonly CommunicationIdentityClient _identityClient;

    public ChatRoomService(IConfiguration config)
    {
        _config = config;

        _acsEndpoint = config["AzureCommunicationServices:Endpoint"]
            ?? throw new InvalidOperationException("ACS endpoint missing.");

        _identityClient = new CommunicationIdentityClient(
            config["AzureCommunicationServices:ConnectionString"]
            ?? throw new InvalidOperationException("ACS connection string missing.")
        );
    }

    // =========================================================
    // SETUP - Run ONCE to create admin user and global thread
    // Call GET /setup, save the result in appsettings
    // =========================================================
    //public async Task<object> SetupGlobalChatAsync()
    //{
    //    try
    //    {
    //        var adminUser = await _identityClient.CreateUserAsync();
    //        Console.WriteLine($"[SETUP] Admin user: {adminUser.Value.Id}");

    //        var adminToken = await _identityClient.GetTokenAsync(
    //            adminUser.Value,
    //            new[] { CommunicationTokenScope.Chat }
    //        );
    //        Console.WriteLine($"[SETUP] Token ok");

    //        var adminClient = new ChatClient(
    //            new Uri(_acsEndpoint),
    //            new CommunicationTokenCredential(adminToken.Value.Token)
    //        );
    //        Console.WriteLine($"[SETUP] Client ok, endpoint: {_acsEndpoint}");

    //        var threadOptions = new CreateChatThreadOptions("General Chat");
    //        var thread = await adminClient.CreateChatThreadAsync(threadOptions);
    //        Console.WriteLine($"[SETUP] Thread: {thread.Value.ChatThread.Id}");

    //        return new
    //        {
    //            AdminUserId = adminUser.Value.Id,
    //            ThreadId = thread.Value.ChatThread.Id
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[SETUP-ERROR]: {ex.GetType().Name}: {ex.Message}");
    //        throw;
    //    }
    //}
    // =========================================================
    // JOIN - Called every time a user wants to join the chat
    // =========================================================
    public async Task<ChatRoomResponseDto> JoinGlobalChatAsync(string username)
    {
        // Read config values
        string globalThreadId = _config["AzureCommunicationServices:GlobalThreadId"]
            ?? throw new InvalidOperationException("GlobalThreadId missing in config.");

        string adminUserId = _config["AzureCommunicationServices:AdminUserId"]
            ?? throw new InvalidOperationException("AdminUserId missing in config.");

        // 1. Create a new ACS identity + token for this user session
        var newUser = await _identityClient.CreateUserAsync();
        var userToken = await _identityClient.GetTokenAsync(
            newUser.Value,
            new[] { CommunicationTokenScope.Chat }
        );

        // 2. Get admin token to add the new user as participant
        var adminToken = await _identityClient.GetTokenAsync(
            new CommunicationUserIdentifier(adminUserId),
            new[] { CommunicationTokenScope.Chat }
        );

        // 3. Use admin client to add new user to the global thread
        var adminClient = new ChatClient(
            new Uri(_acsEndpoint),
            new CommunicationTokenCredential(adminToken.Value.Token)
        );

        var threadClient = adminClient.GetChatThreadClient(globalThreadId);

        try
        {
            var participant = new ChatParticipant(newUser.Value)
            {
                DisplayName = username
            };
            await threadClient.AddParticipantsAsync(new[] { participant });
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            // 409 = Already a participant, safe to ignore
            Console.WriteLine($"[AZURE-CHAT]: User already in thread: {ex.Message}");
        }

        // 4. Return token and thread info to frontend
        return new ChatRoomResponseDto
        {
            AzureThreadId = globalThreadId,
            Endpoint = _acsEndpoint,
            AcsUserId = newUser.Value.Id,
            Token = userToken.Value.Token,
            ExpiresOn = userToken.Value.ExpiresOn
        };
    }
}