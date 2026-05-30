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

public class ChatRoomService (IConfiguration config, ILogger <ChatRoomService> logger)  : IChatRoomService
{
    private readonly string _acsEndpoint = config["AzureCommunicationServices:Endpoint"]
        ?? throw new InvalidOperationException("ACS endpoint missing.");

    private readonly CommunicationIdentityClient _identityClient = new CommunicationIdentityClient(
        config["AzureCommunicationServices:ConnectionString"]
        ?? throw new InvalidOperationException("ACS connection string missing.")

        );

    // SETUP method to create admin user and global thread. Not needed after initial setup.

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


    /// <inheritdoc />
    public async Task<ChatRoomResponseDto> JoinGlobalChatAsync(string username)
    {
        // Read config values - global thread id and admin user id created with initial method and added to env variables
        string globalThreadId = config["AzureCommunicationServices:GlobalThreadId"]
            ?? throw new InvalidOperationException("GlobalThreadId missing in config.");

        string adminUserId = config["AzureCommunicationServices:AdminUserId"]
            ?? throw new InvalidOperationException("AdminUserId missing in config.");

        // Create a new ACS identity + token for this user session
        var newUser = await _identityClient.CreateUserAsync();
        var userToken = await _identityClient.GetTokenAsync(
            newUser.Value,
            new[] { CommunicationTokenScope.Chat }
        );

        //  Get admin token to add the new user as participant
        var adminToken = await _identityClient.GetTokenAsync(
            new CommunicationUserIdentifier(adminUserId),
            new[] { CommunicationTokenScope.Chat }
        );

        //  Use admin client to add new user to the global thread
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
        // If the user is already a participant in the thread when trying to join , -> when rejoining after a page 
        // navigation while the ACS session is still active, ACS returns 409 Conflict.
        // This is expected behavior and safe to ignore
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            logger.LogWarning("User {Username} is already a participant in thread {ThreadId}", username, globalThreadId);
        }

        //  Return token and thread info to frontend
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