﻿using System;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GTANetworkAPI;
using Discord.Net;
using dotenv.net.Utilities;
using Newtonsoft.Json;
using Color = Discord.Color;

namespace Tutorial.Discord
{
    public class DiscordBot
    {
        public DiscordBot() { }

        private static DiscordSocketClient client;
        private static CommandService commands;
        private static IServiceProvider services;
        private static string token = EnvReader.GetStringValue("TOKEN");
        private static ulong guildId = Convert.ToUInt64(EnvReader.GetIntValue("GUILD_ID"));
        private static ulong memberCount = Convert.ToUInt64(EnvReader.GetIntValue("MEMBER_COUNT_ID"));

        public static async Task StartDiscordBot()
        {
            try
            {
                client = new DiscordSocketClient();
                commands = new CommandService();
                services = new ServiceCollection()
                    .AddSingleton(client)
                    .AddSingleton(commands)
                    .BuildServiceProvider();

                client.Log += DiscordBotLog;

                client.Ready += BotIsReady;

                await client.LoginAsync(TokenType.Bot, token);

                await client.StartAsync();

                await Task.Delay(-1);

            }
            catch (Exception) { }
        }

        public async static Task BotIsReady()
        {
            await client.SetGameAsync("mit 0 anderen auf Nemesus.de");

            //await GetUserCount();

            var guild = client.GetGuild(guildId);

            var guildCommand = new SlashCommandBuilder();
            guildCommand.WithName("whitelisttest");
            guildCommand.WithDescription("Das ist unser Whitelist Test Befehl!");

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());

                await client.CreateGlobalApplicationCommandAsync(guildCommand.Build());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }

            client.SlashCommandExecuted += SlashCommandHandler;
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch(command.Data.Name)
            {
                case "whitelisttest":
                    await HandleWhitelistTestCommand(command);
                    break;
            }

        }

        private static async Task HandleWhitelistTestCommand(SocketSlashCommand command)
        {
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(command.User)
                .WithTitle("Whitelisttest")
                .WithDescription("Whitelisttest Befehl erfolgreich ausgeführt!")
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            await command.RespondAsync(embed: embedBuilder.Build());
        }

        public static Task DiscordBotLog(LogMessage message)
        {
            NAPI.Util.ConsoleOutput("DiscordBot: " + message.ToString());
            return Task.CompletedTask;
        }

        public static async Task GetUserCount()
        {
            int count = 0;

            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Exists)
                {
                    count++;
                }
            }

            SocketVoiceChannel MemberCount = client.GetGuild(guildId).GetVoiceChannel(memberCount);

            await MemberCount.ModifyAsync(prop => prop.Name = $"{count} Spieler");
        }

    }
}
