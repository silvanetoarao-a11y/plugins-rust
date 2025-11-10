using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("WelcomePlugin", "YourName", "1.0.0")]
    [Description("Plugin de boas-vindas e comandos úteis para seu servidor")]
    class WelcomePlugin : RustPlugin
    {
        #region Configuração
        
        private Configuration config;

        public class Configuration
        {
            [JsonProperty("Mensagem de boas-vindas")]
            public string WelcomeMessage { get; set; } = "Bem-vindo ao servidor, {player}!";

            [JsonProperty("Mostrar mensagem no chat")]
            public bool ShowInChat { get; set; } = true;

            [JsonProperty("Mostrar popup na tela")]
            public bool ShowPopup { get; set; } = true;

            [JsonProperty("Tempo do popup (segundos)")]
            public float PopupDuration { get; set; } = 5f;

            [JsonProperty("Prefix do chat")]
            public string ChatPrefix { get; set; } = "[Servidor]";

            [JsonProperty("Cor do prefix (hex)")]
            public string PrefixColor { get; set; } = "#00FF00";
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }
            }
            catch
            {
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
            PrintWarning("Arquivo de configuração criado com valores padrão.");
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config, true);
        }

        #endregion

        #region Hooks

        void Init()
        {
            Puts("WelcomePlugin carregado com sucesso!");
        }

        void OnServerInitialized()
        {
            Puts("Servidor inicializado - WelcomePlugin ativo!");
        }

        void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;

            string message = config.WelcomeMessage.Replace("{player}", player.displayName);
            
            if (config.ShowInChat)
            {
                Server.Broadcast($"<color={config.PrefixColor}>{config.ChatPrefix}</color> {message}");
            }

            if (config.ShowPopup)
            {
                player.ShowToast(GameTip.Styles.Blue_Long, "Bem-vindo!", message, config.PopupDuration);
            }
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            
            Server.Broadcast($"<color={config.PrefixColor}>{config.ChatPrefix}</color> {player.displayName} saiu do servidor.");
        }

        void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
        {
            // Você pode adicionar filtros de chat aqui
            // return objeto para bloquear a mensagem
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;
            
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você renasceu! Boa sorte!");
        }

        #endregion

        #region Comandos

        [ChatCommand("ajuda")]
        private void HelpCommand(BasePlayer player, string command, string[] args)
        {
            SendReply(player, $"<color={config.PrefixColor}>=== COMANDOS DISPONÍVEIS ===</color>");
            SendReply(player, "/ajuda - Mostra esta mensagem");
            SendReply(player, "/online - Mostra quantos jogadores estão online");
            SendReply(player, "/regras - Mostra as regras do servidor");
            SendReply(player, "/kit - Recebe um kit inicial (se disponível)");
            SendReply(player, "/pos - Mostra sua posição atual");
        }

        [ChatCommand("online")]
        private void OnlineCommand(BasePlayer player, string command, string[] args)
        {
            int onlinePlayers = BasePlayer.activePlayerList.Count;
            int maxPlayers = ConVar.Server.maxplayers;
            
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Jogadores online: {onlinePlayers}/{maxPlayers}");
            
            if (args.Length > 0 && args[0].ToLower() == "lista")
            {
                SendReply(player, "<color=#FFA500>Lista de jogadores:</color>");
                foreach (var p in BasePlayer.activePlayerList)
                {
                    SendReply(player, $"- {p.displayName}");
                }
            }
        }

        [ChatCommand("regras")]
        private void RulesCommand(BasePlayer player, string command, string[] args)
        {
            SendReply(player, $"<color={config.PrefixColor}>=== REGRAS DO SERVIDOR ===</color>");
            SendReply(player, "1. Seja respeitoso com outros jogadores");
            SendReply(player, "2. Não use cheats ou exploits");
            SendReply(player, "3. Não construa muito perto de outros jogadores");
            SendReply(player, "4. Divirta-se e jogue limpo!");
        }

        [ChatCommand("kit")]
        private void KitCommand(BasePlayer player, string command, string[] args)
        {
            // Verifica se o jogador já recebeu o kit
            if (permission.UserHasPermission(player.UserIDString, "welcomeplugin.kit.received"))
            {
                SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Você já recebeu seu kit inicial!</color>");
                return;
            }

            // Dá itens ao jogador
            GiveStarterKit(player);
            
            // Marca que o jogador recebeu o kit
            permission.GrantUserPermission(player.UserIDString, "welcomeplugin.kit.received", this);
            
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Kit inicial recebido com sucesso!");
        }

        [ChatCommand("pos")]
        private void PositionCommand(BasePlayer player, string command, string[] args)
        {
            var pos = player.transform.position;
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Sua posição: X: {pos.x:F0}, Y: {pos.y:F0}, Z: {pos.z:F0}");
        }

        // Comando apenas para administradores
        [ChatCommand("heal")]
        private void HealCommand(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Você não tem permissão para usar este comando!</color>");
                return;
            }

            BasePlayer target = player;
            
            // Se especificou um jogador como argumento
            if (args.Length > 0)
            {
                target = FindPlayer(args[0]);
                if (target == null)
                {
                    SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Jogador não encontrado!</color>");
                    return;
                }
            }

            target.Heal(target.MaxHealth());
            target.metabolism.calories.value = target.metabolism.calories.max;
            target.metabolism.hydration.value = target.metabolism.hydration.max;
            
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> {target.displayName} foi curado!");
            if (target != player)
            {
                SendReply(target, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você foi curado por um administrador!");
            }
        }

        [ConsoleCommand("welcomeplugin.reload")]
        private void ReloadConfigCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith("Você não tem permissão para usar este comando!");
                return;
            }

            LoadConfig();
            arg.ReplyWith("Configuração do WelcomePlugin recarregada!");
        }

        #endregion

        #region Métodos Auxiliares

        private void GiveStarterKit(BasePlayer player)
        {
            // Lista de itens do kit inicial
            Dictionary<string, int> kitItems = new Dictionary<string, int>
            {
                { "wood", 1000 },
                { "stone", 1000 },
                { "metal.fragments", 500 },
                { "cloth", 100 },
                { "stone.pickaxe", 1 },
                { "hatchet", 1 },
                { "bow.hunting", 1 },
                { "arrow.wooden", 50 },
                { "bandage", 5 }
            };

            foreach (var item in kitItems)
            {
                Item itemToGive = ItemManager.CreateByName(item.Key, item.Value);
                if (itemToGive != null)
                {
                    player.GiveItem(itemToGive);
                }
            }
        }

        private BasePlayer FindPlayer(string nameOrId)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.displayName.ToLower().Contains(nameOrId.ToLower()) ||
                    player.UserIDString == nameOrId)
                {
                    return player;
                }
            }
            return null;
        }

        #endregion
    }
}
