using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Item Skins", "YourName", "1.0.0")]
    [Description("Sistema completo de skins para todos os itens do Rust")]
    public class ItemSkins : RustPlugin
    {
        #region Configuração
        
        private ConfigData configData;
        
        private class ConfigData
        {
            [JsonProperty("Habilitado")]
            public bool Habilitado { get; set; } = true;
            
            [JsonProperty("Permitir todas as skins")]
            public bool PermitirTodasSkins { get; set; } = true;
            
            [JsonProperty("Usar permissões")]
            public bool UsarPermissoes { get; set; } = false;
            
            [JsonProperty("Limite de favoritos")]
            public int LimiteFavoritos { get; set; } = 20;
            
            [JsonProperty("Mostrar IDs das skins")]
            public bool MostrarIDs { get; set; } = true;
            
            [JsonProperty("Cor do UI (RGBA)")]
            public string CorUI { get; set; } = "0.1 0.1 0.1 0.95";
            
            [JsonProperty("Cooldown entre mudanças (segundos)")]
            public float CooldownSegundos { get; set; } = 1f;
        }
        
        protected override void LoadDefaultConfig()
        {
            configData = new ConfigData();
            SaveConfig();
        }
        
        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                {
                    LoadDefaultConfig();
                }
            }
            catch
            {
                PrintWarning("Erro ao carregar config, criando nova...");
                LoadDefaultConfig();
            }
        }
        
        protected override void SaveConfig() => Config.WriteObject(configData);
        
        #endregion
        
        #region Dados
        
        private Dictionary<ulong, PlayerSkinData> playerData = new Dictionary<ulong, PlayerSkinData>();
        private Dictionary<ulong, DateTime> lastSkinChange = new Dictionary<ulong, DateTime>();
        private Dictionary<string, List<ulong>> itemSkins = new Dictionary<string, List<ulong>>();
        
        private class PlayerSkinData
        {
            public List<ulong> Favoritos { get; set; } = new List<ulong>();
            public Dictionary<string, ulong> SkinsPadrao { get; set; } = new Dictionary<string, ulong>();
        }
        
        #endregion
        
        #region Hooks
        
        private void Init()
        {
            if (!configData.Habilitado)
            {
                PrintWarning("Plugin Item Skins desabilitado na configuração!");
                return;
            }
            
            // Registrar comandos
            cmd.AddChatCommand("skin", this, nameof(CmdSkin));
            cmd.AddChatCommand("skins", this, nameof(CmdSkins));
            cmd.AddChatCommand("skinid", this, nameof(CmdSkinId));
            cmd.AddChatCommand("removeskin", this, nameof(CmdRemoveSkin));
            cmd.AddChatCommand("skinfav", this, nameof(CmdSkinFavorito));
            cmd.AddChatCommand("skinauto", this, nameof(CmdSkinAuto));
            
            // Registrar permissões
            permission.RegisterPermission("itemskins.use", this);
            permission.RegisterPermission("itemskins.all", this);
            permission.RegisterPermission("itemskins.admin", this);
            
            LoadData();
            
            Puts("Plugin Item Skins inicializado!");
        }
        
        private void OnServerInitialized()
        {
            CarregarTodasSkins();
            Puts($"Carregadas {itemSkins.Count} categorias de itens com skins!");
        }
        
        private void Unload()
        {
            SaveData();
            
            // Limpar todas as UIs abertas
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player != null)
                {
                    DestruirUI(player);
                }
            }
        }
        
        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null)
            {
                DestruirUI(player);
            }
        }
        
        private void OnItemCraftFinished(ItemCraftTask task, Item item)
        {
            if (item == null || task.owner == null) return;
            
            var player = task.owner;
            if (!playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            if (data.SkinsPadrao.ContainsKey(item.info.shortname))
            {
                ulong skinId = data.SkinsPadrao[item.info.shortname];
                if (skinId > 0)
                {
                    item.skin = skinId;
                    item.MarkDirty();
                }
            }
        }
        
        #endregion
        
        #region Comandos
        
        [ChatCommand("skin")]
        private void CmdSkin(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=yellow>Segure um item na mão para aplicar skin!</color>");
                player.ChatMessage("<color=yellow>Use /skins para ver o menu de skins</color>");
                return;
            }
            
            if (args.Length == 0)
            {
                MostrarUISkinsItem(player, item);
                return;
            }
            
            // Aplicar skin por ID
            ulong skinId;
            if (ulong.TryParse(args[0], out skinId))
            {
                AplicarSkin(player, item, skinId);
            }
            else
            {
                player.ChatMessage("<color=red>ID de skin inválido! Use apenas números.</color>");
            }
        }
        
        [ChatCommand("skins")]
        private void CmdSkins(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=yellow>Segure um item na mão para ver suas skins!</color>");
                return;
            }
            
            MostrarUISkinsItem(player, item);
        }
        
        [ChatCommand("skinid")]
        private void CmdSkinId(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            if (args.Length == 0)
            {
                player.ChatMessage("<color=yellow>Use: /skinid <ID> para aplicar uma skin específica</color>");
                return;
            }
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=yellow>Segure um item na mão!</color>");
                return;
            }
            
            ulong skinId;
            if (!ulong.TryParse(args[0], out skinId))
            {
                player.ChatMessage("<color=red>ID inválido! Use apenas números.</color>");
                return;
            }
            
            AplicarSkin(player, item, skinId);
        }
        
        [ChatCommand("removeskin")]
        private void CmdRemoveSkin(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=yellow>Segure um item na mão!</color>");
                return;
            }
            
            AplicarSkin(player, item, 0);
            player.ChatMessage("<color=green>Skin removida do item!</color>");
        }
        
        [ChatCommand("skinfav")]
        private void CmdSkinFavorito(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            if (args.Length == 0)
            {
                MostrarUIFavoritos(player);
                return;
            }
            
            ulong skinId;
            if (!ulong.TryParse(args[0], out skinId))
            {
                player.ChatMessage("<color=red>ID de skin inválido!</color>");
                return;
            }
            
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerSkinData();
            }
            
            var data = playerData[player.userID];
            
            if (data.Favoritos.Contains(skinId))
            {
                data.Favoritos.Remove(skinId);
                player.ChatMessage($"<color=yellow>Skin {skinId} removida dos favoritos!</color>");
            }
            else
            {
                if (data.Favoritos.Count >= configData.LimiteFavoritos)
                {
                    player.ChatMessage($"<color=red>Você atingiu o limite de {configData.LimiteFavoritos} favoritos!</color>");
                    return;
                }
                data.Favoritos.Add(skinId);
                player.ChatMessage($"<color=green>Skin {skinId} adicionada aos favoritos!</color>");
            }
            
            SaveData();
        }
        
        [ChatCommand("skinauto")]
        private void CmdSkinAuto(BasePlayer player, string command, string[] args)
        {
            if (!TemPermissao(player, "itemskins.use"))
            {
                player.ChatMessage("<color=red>Você não tem permissão para usar este comando!</color>");
                return;
            }
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=yellow>Segure um item na mão!</color>");
                return;
            }
            
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerSkinData();
            }
            
            var data = playerData[player.userID];
            
            if (item.skin == 0)
            {
                player.ChatMessage("<color=red>Este item não tem skin! Aplique uma skin primeiro.</color>");
                return;
            }
            
            data.SkinsPadrao[item.info.shortname] = item.skin;
            player.ChatMessage($"<color=green>Skin {item.skin} definida como padrão para {item.info.displayName.english}!</color>");
            player.ChatMessage("<color=yellow>Novos itens craftados terão esta skin automaticamente!</color>");
            
            SaveData();
        }
        
        #endregion
        
        #region Métodos Principais
        
        private void AplicarSkin(BasePlayer player, Item item, ulong skinId)
        {
            if (item == null || player == null) return;
            
            // Verificar cooldown
            if (lastSkinChange.ContainsKey(player.userID))
            {
                var elapsed = (DateTime.Now - lastSkinChange[player.userID]).TotalSeconds;
                if (elapsed < configData.CooldownSegundos)
                {
                    var remaining = configData.CooldownSegundos - elapsed;
                    player.ChatMessage($"<color=red>Aguarde {remaining:F1}s para mudar a skin novamente!</color>");
                    return;
                }
            }
            
            // Verificar se a skin existe para este item
            if (skinId != 0 && !VerificarSkinDisponivel(item.info.shortname, skinId))
            {
                player.ChatMessage("<color=red>Esta skin não existe para este item!</color>");
                return;
            }
            
            // Aplicar skin
            item.skin = skinId;
            item.MarkDirty();
            
            // Atualizar o item visualmente
            BaseEntity heldEntity = item.GetHeldEntity();
            if (heldEntity != null)
            {
                heldEntity.skinID = skinId;
                heldEntity.SendNetworkUpdate();
            }
            
            lastSkinChange[player.userID] = DateTime.Now;
            
            if (skinId == 0)
            {
                player.ChatMessage("<color=green>Skin removida!</color>");
            }
            else
            {
                player.ChatMessage($"<color=green>Skin {skinId} aplicada em {item.info.displayName.english}!</color>");
            }
        }
        
        private void CarregarTodasSkins()
        {
            itemSkins.Clear();
            
            // Percorrer todos os ItemDefinitions
            foreach (var itemDef in ItemManager.itemList)
            {
                if (itemDef == null) continue;
                
                var skins = GetSkinsParaItem(itemDef);
                if (skins.Count > 0)
                {
                    itemSkins[itemDef.shortname] = skins;
                }
            }
        }
        
        private List<ulong> GetSkinsParaItem(ItemDefinition itemDef)
        {
            List<ulong> skins = new List<ulong>();
            
            if (itemDef.skins != null && itemDef.skins.Length > 0)
            {
                foreach (var skin in itemDef.skins)
                {
                    if (skin.id != 0)
                    {
                        skins.Add((ulong)skin.id);
                    }
                }
            }
            
            // Adicionar skins aprovadas do Workshop
            var workshopSkins = Rust.Workshop.Approved.All
                .Where(skin => skin.Skinnable?.ItemName == itemDef.shortname)
                .Select(skin => skin.WorkskinId);
            
            foreach (var skinId in workshopSkins)
            {
                if (skinId != 0 && !skins.Contains(skinId))
                {
                    skins.Add(skinId);
                }
            }
            
            return skins;
        }
        
        private bool VerificarSkinDisponivel(string itemShortname, ulong skinId)
        {
            if (!itemSkins.ContainsKey(itemShortname)) return false;
            return itemSkins[itemShortname].Contains(skinId);
        }
        
        private bool TemPermissao(BasePlayer player, string perm)
        {
            if (!configData.UsarPermissoes) return true;
            return permission.UserHasPermission(player.UserIDString, perm) || 
                   permission.UserHasPermission(player.UserIDString, "itemskins.all");
        }
        
        #endregion
        
        #region UI
        
        private void MostrarUISkinsItem(BasePlayer player, Item item)
        {
            if (!itemSkins.ContainsKey(item.info.shortname))
            {
                player.ChatMessage($"<color=red>{item.info.displayName.english} não possui skins disponíveis!</color>");
                return;
            }
            
            DestruirUI(player);
            
            var skins = itemSkins[item.info.shortname];
            var container = new CuiElementContainer();
            
            // Painel principal
            string mainPanel = container.Add(new CuiPanel
            {
                Image = { Color = configData.CorUI },
                RectTransform = { AnchorMin = "0.2 0.15", AnchorMax = "0.8 0.85" },
                CursorEnabled = true
            }, "Overlay", "SkinsPanel");
            
            // Título
            container.Add(new CuiLabel
            {
                Text = { 
                    Text = $"SKINS - {item.info.displayName.english.ToUpper()}", 
                    FontSize = 20, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform = { AnchorMin = "0 0.92", AnchorMax = "1 1" }
            }, mainPanel);
            
            // Info
            container.Add(new CuiLabel
            {
                Text = { 
                    Text = $"{skins.Count} skins disponíveis | Clique para aplicar", 
                    FontSize = 12, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.8 0.8 0.8 1"
                },
                RectTransform = { AnchorMin = "0 0.88", AnchorMax = "1 0.92" }
            }, mainPanel);
            
            // Botão remover skin
            container.Add(new CuiButton
            {
                Button = { Command = "itemskins.apply 0", Color = "0.8 0.2 0.2 1" },
                RectTransform = { AnchorMin = "0.02 0.02", AnchorMax = "0.15 0.08" },
                Text = { Text = "Remover Skin", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, mainPanel);
            
            // Botão fechar
            container.Add(new CuiButton
            {
                Button = { Command = "itemskins.close", Color = "0.5 0.5 0.5 1" },
                RectTransform = { AnchorMin = "0.85 0.02", AnchorMax = "0.98 0.08" },
                Text = { Text = "Fechar", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, mainPanel);
            
            // Grid de skins
            int columns = 6;
            int rows = 8;
            float buttonWidth = 0.15f;
            float buttonHeight = 0.09f;
            float spacingX = 0.01f;
            float spacingY = 0.01f;
            float startX = 0.02f;
            float startY = 0.78f;
            
            int skinsPerPage = columns * rows;
            int totalPages = Mathf.CeilToInt((float)skins.Count / skinsPerPage);
            
            for (int i = 0; i < Math.Min(skins.Count, skinsPerPage); i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                float xMin = startX + (col * (buttonWidth + spacingX));
                float xMax = xMin + buttonWidth;
                float yMax = startY - (row * (buttonHeight + spacingY));
                float yMin = yMax - buttonHeight;
                
                ulong skinId = skins[i];
                bool isFavorito = playerData.ContainsKey(player.userID) && 
                                 playerData[player.userID].Favoritos.Contains(skinId);
                
                string buttonColor = isFavorito ? "0.8 0.6 0.2 1" : "0.3 0.3 0.3 1";
                
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.apply {skinId}", Color = buttonColor },
                    RectTransform = { AnchorMin = $"{xMin} {yMin}", AnchorMax = $"{xMax} {yMax}" },
                    Text = { 
                        Text = configData.MostrarIDs ? $"ID: {skinId}" : $"Skin {i+1}", 
                        FontSize = 10, 
                        Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, mainPanel);
                
                // Botão favorito (estrela)
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.fav {skinId}", Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = $"{xMin} {yMax - 0.02f}", AnchorMax = $"{xMin + 0.02f} {yMax}" },
                    Text = { 
                        Text = isFavorito ? "★" : "☆", 
                        FontSize = 14, 
                        Align = TextAnchor.MiddleCenter,
                        Color = isFavorito ? "1 0.8 0 1" : "0.5 0.5 0.5 1"
                    }
                }, mainPanel);
            }
            
            CuiHelper.AddUi(player, container);
        }
        
        private void MostrarUIFavoritos(BasePlayer player)
        {
            if (!playerData.ContainsKey(player.userID) || playerData[player.userID].Favoritos.Count == 0)
            {
                player.ChatMessage("<color=yellow>Você não tem skins favoritas!</color>");
                player.ChatMessage("<color=yellow>Use /skins e clique na estrela para adicionar favoritos!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            
            player.ChatMessage("<color=cyan>=== SKINS FAVORITAS ===</color>");
            foreach (var skinId in data.Favoritos)
            {
                player.ChatMessage($"<color=yellow>ID: {skinId}</color>");
            }
            player.ChatMessage($"<color=cyan>Total: {data.Favoritos.Count}/{configData.LimiteFavoritos}</color>");
        }
        
        private void DestruirUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "SkinsPanel");
        }
        
        #endregion
        
        #region Console Commands
        
        [ConsoleCommand("itemskins.apply")]
        private void CmdApplySkin(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            if (arg.Args == null || arg.Args.Length == 0) return;
            
            ulong skinId;
            if (!ulong.TryParse(arg.Args[0], out skinId)) return;
            
            var item = player.GetActiveItem();
            if (item == null)
            {
                player.ChatMessage("<color=red>Segure o item na mão!</color>");
                return;
            }
            
            AplicarSkin(player, item, skinId);
            
            // Atualizar UI
            timer.Once(0.1f, () => {
                if (player != null && player.IsConnected)
                {
                    MostrarUISkinsItem(player, item);
                }
            });
        }
        
        [ConsoleCommand("itemskins.fav")]
        private void CmdToggleFav(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            if (arg.Args == null || arg.Args.Length == 0) return;
            
            ulong skinId;
            if (!ulong.TryParse(arg.Args[0], out skinId)) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerSkinData();
            }
            
            var data = playerData[player.userID];
            
            if (data.Favoritos.Contains(skinId))
            {
                data.Favoritos.Remove(skinId);
                player.ChatMessage($"<color=yellow>Skin {skinId} removida dos favoritos!</color>");
            }
            else
            {
                if (data.Favoritos.Count >= configData.LimiteFavoritos)
                {
                    player.ChatMessage($"<color=red>Limite de favoritos atingido!</color>");
                    return;
                }
                data.Favoritos.Add(skinId);
                player.ChatMessage($"<color=green>Skin {skinId} adicionada aos favoritos!</color>");
            }
            
            SaveData();
            
            // Atualizar UI
            var item = player.GetActiveItem();
            if (item != null)
            {
                timer.Once(0.1f, () => {
                    if (player != null && player.IsConnected)
                    {
                        MostrarUISkinsItem(player, item);
                    }
                });
            }
        }
        
        [ConsoleCommand("itemskins.close")]
        private void CmdCloseUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            DestruirUI(player);
        }
        
        #endregion
        
        #region Data
        
        private void LoadData()
        {
            try
            {
                playerData = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, PlayerSkinData>>("ItemSkins_Data") 
                             ?? new Dictionary<ulong, PlayerSkinData>();
            }
            catch
            {
                playerData = new Dictionary<ulong, PlayerSkinData>();
            }
        }
        
        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("ItemSkins_Data", playerData);
        }
        
        #endregion
    }
}
