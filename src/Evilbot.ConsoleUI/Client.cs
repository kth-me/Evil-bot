﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Evilbot.Common;
using Evilbot.Common.Models;

namespace Evilbot.ConsoleUI
{
    internal class Client
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ConfigModel _config;
        private readonly PluginLoader _pluginLoader;

        //private readonly LogHandler _logger;
        private readonly IServiceProvider _services;

        public Client(CommandService commands = null, ConfigModel config = null, LogHandler logger = null, PluginLoader pluginLoader = null)
        {
            // Create new DiscordClient
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 500
            });

            // Create new CommandService
            _commands = commands ?? new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            });

            // Set up config and services for use in initialization
            _config = config ?? new ConfigHandler().GetConfig();
            //_logger = logger ?? new LogHandler();
            _services = ConfigureServices();
        }

        public async Task InitializeAsync()
        {
            // Initialize CommandHandler, LogHandler, and DiscordEventHandler services
            await _services.GetRequiredService<CommandHandler>().InitializeAsync();
            await _services.GetRequiredService<LogHandler>().InitializeAsync();
            await _services.GetRequiredService<PluginLoader>().LoadPlugins();
            await _services.GetRequiredService<DiscordEventHandler>().InitializeAsync();

            // Login with client and start
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            // Prevent bot from self-terminating
            await Task.Delay(-1);
        }

        // Used to add any services to the DI Service Provider
        // These services are then injected wherever they're needed
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<CommandHandler>()
                .AddSingleton<LogHandler>()
                .AddSingleton<PluginLoader>()
                .AddSingleton<DiscordEventHandler>()
                .AddSingleton<ConfigHandler>()
                .BuildServiceProvider();
        }
    }
}