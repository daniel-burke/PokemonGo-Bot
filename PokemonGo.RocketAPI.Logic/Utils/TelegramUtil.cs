﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using AllEnum;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logic.Utils;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Logic;
using PokemonGo.RocketAPI.Helpers;

namespace PokemonGo.RocketAPI.Logic.Utils
{
    public class TelegramUtil
    {

        private Client _client;
        private Inventory _inventory;

        private Telegram.Bot.TelegramBotClient _telegram;

        private readonly ISettings _clientSettings;

        private long chatid = -1;
        private bool livestats = false;

        private bool informations = false;

        public TelegramUtil(Client client, Telegram.Bot.TelegramBotClient telegram, ISettings settings, Inventory inv)
        {
            _client = client;
            _telegram = telegram;
            _clientSettings = settings;
            _inventory = inv;
            DoLiveStats();
            DoInformation();
        }

        public Telegram.Bot.TelegramBotClient getClient()
        {
            return _telegram;
        }

        public async void DoLiveStats()
        {
            if (chatid != -1 && livestats)
            {
                var usage = "";
                var inventory = await _client.GetInventory();
                var profil = await _client.GetProfile();
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                foreach (var c in stats)
                {
                    if (c != null)
                    {
                        int l = c.Level;

                        usage += "\nNickname: " + profil.Profile.Username +
                            "\nLevel: " + c.Level
                            + "\nEXP Needed: " + ((c.NextLevelXp - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level))
                            + "\nCurrent EXP: " + ((c.Experience - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level))
                            + "\nEXP to Level up: " + ((c.NextLevelXp) - (c.Experience))
                            + "\nKM walked: " + c.KmWalked
                            + "\nPokeStops visited: " + c.PokeStopVisits
                            + "\nStardust: " + profil.Profile.Currency.ToArray()[1].Amount;
                    }
                }
                
                await _telegram.SendTextMessageAsync(chatid, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
            await Task.Delay(5000);
            DoLiveStats();
        }

         int level;
        
        public async void DoInformation()
        {
            if (chatid != -1 && informations)
            {
                int current = 0;
                var usage = "";
                var inventory = await _client.GetInventory();
                var profil = await _client.GetProfile();
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                foreach (var c in stats)
                {
                    if (c != null)
                    {
                        current = c.Level;
                    }
                }

                if (current != level)
                {
                    level = current;
                    usage = "You got Level Up! Your new Level is now " + level + "!";
                    await _telegram.SendTextMessageAsync(chatid, usage,
                   replyMarkup: new ReplyKeyboardHide());
                }
            }
            await Task.Delay(5000);
            DoInformation();
        }


        public async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            
            if (message == null || message.Type != MessageType.TextMessage) return;

            Logger.ColoredConsoleWrite(ConsoleColor.Red, "[TelegramAPI]Got Request from " + messageEventArgs.Message.From.Username + " | " + message.Text);

            if (messageEventArgs.Message.From.Username != _clientSettings.TelegramName)
            {
                var usage = "I dont hear at you!";
                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                   replyMarkup: new ReplyKeyboardHide());
                return;
            }

            if (message.Text.StartsWith("/stats")) // send inline keyboard
            {

                var usage = "";
                var inventory = await _client.GetInventory();
                var profil = await _client.GetProfile();
                var stats = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData.PlayerStats).ToArray();
                foreach (var c in stats)
                {
                    if (c != null)
                    {
                        int l = c.Level;

                        usage += "\nNickname: " + profil.Profile.Username + 
                            "\nLevel: " + c.Level
                            + "\nEXP Needed: " + ((c.NextLevelXp - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level))
                            + "\nCurrent EXP: " + ((c.Experience - c.PrevLevelXp) - StringUtils.getExpDiff(c.Level))
                            + "\nEXP to Level up: " + ((c.NextLevelXp) - (c.Experience))
                            + "\nKM walked: " + c.KmWalked
                            + "\nPokeStops visited: " + c.PokeStopVisits
                            + "\nStardust: " + profil.Profile.Currency.ToArray()[1].Amount;
                    }
                }
                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
            else if (message.Text.StartsWith("/livestats")) // send custom keyboard
            {
                var usage = "";
                if (livestats)
                {
                    usage = "Disabled Live Stats.";
                    livestats = false;
                    chatid = -1;
                } else
                {
                    usage = "Enabled Live Stats.";
                    livestats = true;
                    chatid = message.Chat.Id;
                }
                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
            else if (message.Text.StartsWith("/informations")) // send custom keyboard
            {
                var usage = "";
                if (livestats)
                {
                    usage = "Disabled Informations.";
                    informations = false;
                    chatid = -1;
                }
                else
                {
                    usage = "Enabled Informations.";
                    informations = true;
                    chatid = message.Chat.Id;
                }
                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
            else if (message.Text.StartsWith("/evolve"))
            {
                var usage = "Evolving the shit out!";
                
                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                   replyMarkup: new ReplyKeyboardHide());
            } else if (message.Text.StartsWith("/top"))
            {
                int shows;
                try
                {
                    shows = int.Parse(message.Text.Replace("/top", "").Replace(" ", ""));
                } catch (Exception)
                {
                    var usage = "Error! This is not a Number: " + message.Text.Replace("/top", "").Replace(" ", "") + "!";

                    await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                       replyMarkup: new ReplyKeyboardHide());
                    return;
                }

                await _telegram.SendTextMessageAsync(message.Chat.Id, "Showing " + shows + " Pokemons...\nSorting..." ,
                       replyMarkup: new ReplyKeyboardHide());
                
                var myPokemons = await _inventory.GetPokemons();
                myPokemons = myPokemons.OrderByDescending(x => x.Cp);
                 

                var u = "Top " + shows + " Pokemons!";

                int count = 0;
                foreach (var pokemon in myPokemons)
                {
                    if (count == shows)
                        break;

                    u = u + "\n" + pokemon.PokemonId + " (" + StringUtils.getPokemonNameGer(pokemon.PokemonId) + ")  |  CP: " + pokemon.Cp;
                    count++;
                }

                await _telegram.SendTextMessageAsync(message.Chat.Id, u, replyMarkup: new ReplyKeyboardHide());

            } else if (message.Text.StartsWith("/forceevolve"))
            {
                var pokemonToEvolve = await _inventory.GetPokemonToEvolve(null);
                if (pokemonToEvolve.Count() > 30)
                {
                    // Use EGG - need to add this shit
                }
                foreach (var pokemon in pokemonToEvolve)
                {

                    if (!_clientSettings.pokemonsToEvolve.Contains(pokemon.PokemonId))
                    {
                        continue;
                    }
                    var evolvePokemonOutProto = await _client.EvolvePokemon((ulong)pokemon.Id);

                    if (evolvePokemonOutProto.Result == EvolvePokemonOut.Types.EvolvePokemonStatus.PokemonEvolvedSuccess)
                    {
                        await _telegram.SendTextMessageAsync(message.Chat.Id, $"Evolved {pokemon.PokemonId} successfully for {evolvePokemonOutProto.ExpAwarded}xp", replyMarkup: new ReplyKeyboardHide());
                    }
                    else
                    {
                        await _telegram.SendTextMessageAsync(message.Chat.Id, $"Failed to evolve {pokemon.PokemonId}. EvolvePokemonOutProto.Result was {evolvePokemonOutProto.Result}, stopping evolving {pokemon.PokemonId}", replyMarkup: new ReplyKeyboardHide());
                    }
                    await RandomHelper.RandomDelay(1000, 2000);
                }
                await _telegram.SendTextMessageAsync(message.Chat.Id, "Done.", replyMarkup: new ReplyKeyboardHide());
            }
            else
            {
                var usage = @"Usage:
                    /stats   - Get Current Stats
                    /livestats - Enable/Disable Live Stats
                    /informations - Enable/Disable Informations
                    /top <HowMany?> - Outputs Top (?) Pokemons
                    /forceevolve - Forces Evolve";

                await _telegram.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
        }


        public async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await _telegram.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
