using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;  // ← ADICIONADO PARA CORRIGIR O ERRO

namespace Oxide.Plugins
{
    [Info("PvP Training", "Seu Nome", "2.0.0")]
    [Description("Plugin de treino PvP com sistema de ilhas e bots AI")]
    public class PvPTraining : RustPlugin
    {
        #region Configuração
        
        private ConfigData configData;
        
        private class ConfigData
        {
            public bool Habilitado { get; set; } = true;
            public bool RespawnAutomatico { get; set; } = true;
            public float TempoRespawnSegundos { get; set; } = 3f;
            public bool DarArmasAoEntrar { get; set; } = true;
            public bool DarArmadurasAoEntrar { get; set; } = true;
            public bool DarMedicamentosAoEntrar { get; set; } = true;
            public bool PvPAtivado { get; set; } = true;
            public bool DanoEntreJogadores { get; set; } = true;
            public float TamanhoIlha { get; set; } = 100f;
            public float DistanciaEntreIlhas { get; set; } = 500f;
            public float AlturaIlha { get; set; } = 50f;
            public int QuantidadeBotsFacil { get; set; } = 3;
            public int QuantidadeBotsMedio { get; set; } = 5;
            public int QuantidadeBotsDificil { get; set; } = 7;
            public List<string> ArmasDisponiveis { get; set; } = new List<string>
            {
                "rifle.ak",
                "rifle.lr300",
                "smg.thompson"
            };
            public int QuantidadeMunicao { get; set; } = 500;
            public int QuantidadeMedkit { get; set; } = 10;
            public int QuantidadeBandagem { get; set; } = 20;
        }
        
        protected override void LoadDefaultConfig()
        {
            configData = new ConfigData();
            SaveConfig();
        }
        
        protected override void LoadConfig()
        {
            base.LoadConfig();
            configData = Config.ReadObject<ConfigData>();
            if (configData == null)
            {
                LoadDefaultConfig();
            }
        }
        
        protected override void SaveConfig() => Config.WriteObject(configData);
        
        #endregion
        
        #region Dados do Jogador
        
        private Dictionary<ulong, PlayerPvPData> playerData = new Dictionary<ulong, PlayerPvPData>();
        private Dictionary<Vector3, IlhaData> ilhasAtivas = new Dictionary<Vector3, IlhaData>();
        private Dictionary<Vector3, IlhaBotData> ilhasBots = new Dictionary<Vector3, IlhaBotData>();
        
        private class PlayerPvPData
        {
            public bool EmAreaPvP { get; set; } = false;
            public Vector3 PosicaoOriginal { get; set; }
            public Vector3 PosicaoIlha { get; set; }
            public string TipoIlha { get; set; } = "normal"; // normal, facil, medio, dificil
            public int Kills { get; set; } = 0;
            public int Deaths { get; set; } = 0;
            public DateTime UltimaMorte { get; set; }
        }
        
        private class IlhaData
        {
            public Vector3 Posicao { get; set; }
            public List<ulong> PlayersNaIlha { get; set; } = new List<ulong>();
            public DateTime DataCriacao { get; set; } = DateTime.Now;
        }
        
        private class IlhaBotData
        {
            public Vector3 Posicao { get; set; }
            public string Dificuldade { get; set; } // facil, medio, dificil
            public List<BaseNpc> Bots { get; set; } = new List<BaseNpc>();
            public List<ulong> PlayersNaIlha { get; set; } = new List<ulong>();
        }
        
        #endregion
        
        #region Hooks
        
        private void Init()
        {
            if (!configData.Habilitado)
            {
                PrintWarning("Plugin PvP Training desabilitado na configuração!");
                return;
            }
            
            cmd.AddChatCommand("pvp", this, nameof(CmdPvP));
            cmd.AddChatCommand("pvpbots", this, nameof(CmdPvPBots));
            cmd.AddChatCommand("sairpvp", this, nameof(CmdSairPvP));
            cmd.AddChatCommand("stats", this, nameof(CmdStats));
            
            PrintWarning("Plugin PvP Training inicializado!");
        }
        
        private void OnServerInitialized()
        {
            PrintWarning($"Plugin PvP Training {Version} está ativo.");
            CriarIlhasBots();
        }
        
        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null && playerData.ContainsKey(player.userID))
            {
                var data = playerData[player.userID];
                if (data.EmAreaPvP)
                {
                    RemoverJogadorDaIlha(player.userID, data.PosicaoIlha);
                    if (data.TipoIlha == "normal")
                    {
                        RemoverIlha(player.userID);
                    }
                    data.EmAreaPvP = false;
                }
            }
        }
        
        private object OnPlayerAttack(BasePlayer attacker, HitInfo info)
        {
            if (!configData.DanoEntreJogadores) return null;
            
            var victim = info?.HitEntity as BasePlayer;
            if (victim == null || attacker == null) return null;
            
            if (!playerData.ContainsKey(attacker.userID)) return null;
            
            var attackerData = playerData[attacker.userID];
            if (!attackerData.EmAreaPvP) return null;
            
            // Verificar se é um bot
            var bot = victim.GetComponent<BaseNpc>();
            if (bot != null)
            {
                return null; // Permite dano em bots
            }
            
            if (!playerData.ContainsKey(victim.userID)) return null;
            
            var victimData = playerData[victim.userID];
            if (!victimData.EmAreaPvP) return null;
            
            return null; // Permite o dano
        }
        
        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (player == null || !playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            if (!data.EmAreaPvP) return;
            
            data.Deaths++;
            data.UltimaMorte = DateTime.Now;
            
            var attacker = info?.InitiatorPlayer;
            if (attacker != null && attacker != player && playerData.ContainsKey(attacker.userID))
            {
                var attackerData = playerData[attacker.userID];
                if (attackerData.EmAreaPvP)
                {
                    attackerData.Kills++;
                    attacker.ChatMessage($"<color=green>Você eliminou {player.displayName}!</color>");
                    attacker.ChatMessage($"<color=yellow>Kills: {attackerData.Kills} | Deaths: {attackerData.Deaths}</color>");
                }
            }
            
            // Verificar se foi morto por bot
            var bot = info?.Initiator as BaseNpc;
            if (bot != null && playerData.ContainsKey(player.userID))
            {
                player.ChatMessage($"<color=red>Você foi eliminado por um bot!</color>");
            }
            else
            {
                player.ChatMessage($"<color=red>Você foi eliminado!</color>");
            }
            
            player.ChatMessage($"<color=yellow>Kills: {data.Kills} | Deaths: {data.Deaths}</color>");
            
            if (configData.RespawnAutomatico)
            {
                timer.Once(configData.TempoRespawnSegundos, () =>
                {
                    if (player != null && !player.IsDead && data.EmAreaPvP)
                    {
                        RespawnarJogador(player);
                    }
                });
            }
        }
        
        #endregion
        
        #region Comandos
        
        [ChatCommand("pvp")]
        private void CmdPvP(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerPvPData();
            }
            
            var data = playerData[player.userID];
            
            if (data.EmAreaPvP)
            {
                player.ChatMessage("<color=yellow>Você já está na área de PvP!</color>");
                MostrarStats(player);
                return;
            }
            
            EntrarAreaPvP(player);
        }
        
        [ChatCommand("pvpbots")]
        private void CmdPvPBots(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (args == null || args.Length == 0)
            {
                MostrarMenuBots(player);
                return;
            }
            
            string dificuldade = args[0].ToLower();
            if (dificuldade != "facil" && dificuldade != "medio" && dificuldade != "dificil")
            {
                player.ChatMessage("<color=red>Dificuldade inválida! Use: facil, medio ou dificil</color>");
                return;
            }
            
            EntrarIlhaBots(player, dificuldade);
        }
        
        [ChatCommand("sairpvp")]
        private void CmdSairPvP(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                player.ChatMessage("<color=red>Você não está na área de PvP!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            if (!data.EmAreaPvP)
            {
                player.ChatMessage("<color=red>Você não está na área de PvP!</color>");
                return;
            }
            
            SairAreaPvP(player);
        }
        
        [ChatCommand("stats")]
        private void CmdStats(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            MostrarStats(player);
        }
        
        #endregion
        
        #region Métodos PvP
        
        private void EntrarAreaPvP(BasePlayer player)
        {
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerPvPData();
            }
            
            var data = playerData[player.userID];
            data.PosicaoOriginal = player.transform.position;
            data.EmAreaPvP = true;
            data.TipoIlha = "normal";
            
            // Criar ilha para o jogador
            CriarIlha(player);
            
            // Adicionar à contagem
            AdicionarJogadorNaIlha(player.userID, data.PosicaoIlha);
            
            // Mostrar estatísticas de ilhas
            MostrarEstatisticasIlhas(player);
            
            // Limpar inventário
            player.inventory.Strip();
            
            // Dar itens de PvP
            if (configData.DarArmasAoEntrar)
            {
                DarArmas(player);
            }
            
            if (configData.DarArmadurasAoEntrar)
            {
                DarArmaduras(player);
            }
            
            if (configData.DarMedicamentosAoEntrar)
            {
                DarMedicamentos(player);
            }
            
            // Dar recursos básicos
            var madeira = ItemManager.CreateByName("wood", 1000);
            if (madeira != null) player.inventory.GiveItem(madeira);
            
            var pedra = ItemManager.CreateByName("stones", 1000);
            if (pedra != null) player.inventory.GiveItem(pedra);
            
            var metal = ItemManager.CreateByName("metal.fragments", 500);
            if (metal != null) player.inventory.GiveItem(metal);
            
            player.ChatMessage("<color=green>Você entrou na área de PvP!</color>");
            player.ChatMessage("<color=yellow>Você foi teleportado para sua ilha de treino!</color>");
            player.ChatMessage("<color=yellow>Use /sairpvp para sair</color>");
            player.ChatMessage("<color=yellow>Use /stats para ver suas estatísticas</color>");
            
            MostrarStats(player);
        }
        
        private void EntrarIlhaBots(BasePlayer player, string dificuldade)
        {
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerPvPData();
            }
            
            var data = playerData[player.userID];
            
            if (data.EmAreaPvP && data.TipoIlha != "normal")
            {
                RemoverJogadorDaIlha(player.userID, data.PosicaoIlha);
            }
            
            data.PosicaoOriginal = player.transform.position;
            data.EmAreaPvP = true;
            data.TipoIlha = dificuldade;
            
            // Encontrar ou criar ilha de bots
            Vector3 posicaoIlha = EncontrarIlhaBots(dificuldade);
            data.PosicaoIlha = posicaoIlha;
            
            // Adicionar jogador à ilha de bots
            if (!ilhasBots.ContainsKey(posicaoIlha))
            {
                ilhasBots[posicaoIlha] = new IlhaBotData
                {
                    Posicao = posicaoIlha,
                    Dificuldade = dificuldade
                };
            }
            
            ilhasBots[posicaoIlha].PlayersNaIlha.Add(player.userID);
            
            // Teleportar jogador
            Vector3 posicaoJogador = posicaoIlha + Vector3.up * 2f;
            player.Teleport(posicaoJogador);
            
            // Limpar inventário
            player.inventory.Strip();
            
            // Dar itens de PvP
            if (configData.DarArmasAoEntrar)
            {
                DarArmas(player);
            }
            
            if (configData.DarArmadurasAoEntrar)
            {
                DarArmaduras(player);
            }
            
            if (configData.DarMedicamentosAoEntrar)
            {
                DarMedicamentos(player);
            }
            
            player.ChatMessage($"<color=green>Você entrou na ilha de bots - Dificuldade: {dificuldade.ToUpper()}!</color>");
            player.ChatMessage("<color=yellow>Use /sairpvp para sair</color>");
            
            MostrarEstatisticasIlhas(player);
        }
        
        private void SairAreaPvP(BasePlayer player)
        {
            if (!playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            data.EmAreaPvP = false;
            
            // Remover da ilha
            RemoverJogadorDaIlha(player.userID, data.PosicaoIlha);
            
            // Remover ilha se for normal
            if (data.TipoIlha == "normal")
            {
                RemoverIlha(player.userID);
            }
            
            // Teleportar de volta
            player.Teleport(data.PosicaoOriginal);
            data.PosicaoIlha = Vector3.zero;
            
            // Limpar inventário
            player.inventory.Strip();
            
            // Restaurar vida
            player.Heal(player.MaxHealth());
            
            player.ChatMessage("<color=green>Você saiu da área de PvP!</color>");
            MostrarStats(player);
        }
        
        private void RespawnarJogador(BasePlayer player)
        {
            if (player == null || player.IsDead) return;
            
            if (!playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            if (!data.EmAreaPvP) return;
            
            player.Respawn();
            player.Heal(player.MaxHealth());
            
            // Teleportar de volta para a ilha
            if (data.PosicaoIlha != Vector3.zero)
            {
                Vector3 posicaoRespawn = data.PosicaoIlha + Vector3.up * 2f;
                player.Teleport(posicaoRespawn);
            }
            
            // Dar itens novamente
            if (configData.DarArmasAoEntrar)
            {
                DarArmas(player);
            }
            
            if (configData.DarArmadurasAoEntrar)
            {
                DarArmaduras(player);
            }
            
            if (configData.DarMedicamentosAoEntrar)
            {
                DarMedicamentos(player);
            }
            
            player.ChatMessage("<color=yellow>Você respawnou! Continue lutando!</color>");
        }
        
        private void MostrarEstatisticasIlhas(BasePlayer player)
        {
            int totalIlhas = ilhasAtivas.Count + ilhasBots.Count;
            int totalPlayers = 0;
            
            foreach (var ilha in ilhasAtivas.Values)
            {
                totalPlayers += ilha.PlayersNaIlha.Count;
            }
            
            foreach (var ilha in ilhasBots.Values)
            {
                totalPlayers += ilha.PlayersNaIlha.Count;
            }
            
            player.ChatMessage("<color=cyan>=== ESTATÍSTICAS DAS ILHAS ===");
            player.ChatMessage($"<color=white>Total de Ilhas Ativas: <color=yellow>{totalIlhas}</color>");
            player.ChatMessage($"<color=white>Total de Players nas Ilhas: <color=yellow>{totalPlayers}</color>");
            player.ChatMessage($"<color=white>Ilhas Normais: <color=green>{ilhasAtivas.Count}</color>");
            player.ChatMessage($"<color=white>Ilhas com Bots: <color=orange>{ilhasBots.Count}</color>");
            player.ChatMessage("<color=cyan>============================</color>");
        }
        
        private void MostrarMenuBots(BasePlayer player)
        {
            var container = new CuiElementContainer();
            string panel = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95" },
                RectTransform = { AnchorMin = "0.3 0.3", AnchorMax = "0.7 0.7" },
                CursorEnabled = true
            }, "Overlay", "BotsMenuPanel");
            
            container.Add(new CuiLabel
            {
                Text = { Text = "SELECIONE A DIFICULDADE", FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.75", AnchorMax = "1 1" }
            }, panel);
            
            container.Add(new CuiButton
            {
                Button = { Command = "pvpbots facil", Color = "0.2 0.8 0.2 1" },
                RectTransform = { AnchorMin = "0.1 0.55", AnchorMax = "0.9 0.7" },
                Text = { Text = "FÁCIL - 3 Bots", FontSize = 18, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, panel);
            
            container.Add(new CuiButton
            {
                Button = { Command = "pvpbots medio", Color = "0.8 0.6 0.2 1" },
                RectTransform = { AnchorMin = "0.1 0.35", AnchorMax = "0.9 0.5" },
                Text = { Text = "MÉDIO - 5 Bots", FontSize = 18, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, panel);
            
            container.Add(new CuiButton
            {
                Button = { Command = "pvpbots dificil", Color = "0.8 0.2 0.2 1" },
                RectTransform = { AnchorMin = "0.1 0.15", AnchorMax = "0.9 0.3" },
                Text = { Text = "DIFÍCIL - 7 Bots", FontSize = 18, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, panel);
            
            container.Add(new CuiButton
            {
                Button = { Command = "pvpbots.close", Color = "0.5 0.5 0.5 1" },
                RectTransform = { AnchorMin = "0.3 0.02", AnchorMax = "0.7 0.1" },
                Text = { Text = "FECHAR", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, panel);
            
            CuiHelper.AddUi(player, container);
        }
        
        private void DarArmas(BasePlayer player)
        {
            if (player == null || player.inventory == null) return;
            
            foreach (var armaNome in configData.ArmasDisponiveis)
            {
                var arma = ItemManager.CreateByName(armaNome);
                if (arma != null)
                {
                    player.inventory.GiveItem(arma);
                    
                    string municaoNome = GetMunicaoParaArma(armaNome);
                    if (!string.IsNullOrEmpty(municaoNome))
                    {
                        var municao = ItemManager.CreateByName(municaoNome, configData.QuantidadeMunicao);
                        if (municao != null)
                        {
                            player.inventory.GiveItem(municao);
                        }
                    }
                }
            }
        }
        
        private void DarArmaduras(BasePlayer player)
        {
            if (player == null || player.inventory == null) return;
            
            var capacete = ItemManager.CreateByName("metal.facemask");
            if (capacete != null) player.inventory.GiveItem(capacete);
            
            var colete = ItemManager.CreateByName("metal.plate.torso");
            if (colete != null) player.inventory.GiveItem(colete);
            
            var calca = ItemManager.CreateByName("metal.plate.pants");
            if (calca != null) player.inventory.GiveItem(calca);
        }
        
        private void DarMedicamentos(BasePlayer player)
        {
            if (player == null || player.inventory == null) return;
            
            var medkit = ItemManager.CreateByName("syringe.medical", configData.QuantidadeMedkit);
            if (medkit != null) player.inventory.GiveItem(medkit);
            
            var bandagem = ItemManager.CreateByName("bandage", configData.QuantidadeBandagem);
            if (bandagem != null) player.inventory.GiveItem(bandagem);
        }
        
        private string GetMunicaoParaArma(string armaNome)
        {
            switch (armaNome.ToLower())
            {
                case "rifle.ak":
                case "rifle.lr300":
                case "smg.thompson":
                    return "ammo.rifle";
                case "pistol.eoka":
                case "pistol.revolver":
                    return "ammo.pistol";
                case "crossbow":
                    return "arrow.wooden";
                default:
                    return "ammo.rifle";
            }
        }
        
        private void MostrarStats(BasePlayer player)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                player.ChatMessage("<color=yellow>Nenhuma estatística disponível. Use /pvp para entrar na área de PvP!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            
            float kd = data.Deaths > 0 ? (float)data.Kills / data.Deaths : data.Kills;
            
            player.ChatMessage("<color=cyan>=== ESTATÍSTICAS PvP ===");
            player.ChatMessage($"<color=white>Kills: <color=green>{data.Kills}</color>");
            player.ChatMessage($"<color=white>Deaths: <color=red>{data.Deaths}</color>");
            player.ChatMessage($"<color=white>K/D Ratio: <color=yellow>{kd:F2}</color>");
            player.ChatMessage($"<color=white>Status: <color={(data.EmAreaPvP ? "green" : "red")}>{(data.EmAreaPvP ? "Em PvP" : "Fora de PvP")}</color>");
            player.ChatMessage("<color=cyan>======================</color>");
        }
        
        #endregion
        
        #region Sistema de Ilhas
        
        private void CriarIlha(BasePlayer player)
        {
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerPvPData();
            }
            
            var data = playerData[player.userID];
            
            int ilhaIndex = (int)(player.userID % 1000);
            float angulo = (ilhaIndex * 15f) * Mathf.Deg2Rad;
            float distancia = configData.DistanciaEntreIlhas + (ilhaIndex * 10f);
            
            Vector3 posicaoIlha = new Vector3(
                Mathf.Cos(angulo) * distancia,
                configData.AlturaIlha,
                Mathf.Sin(angulo) * distancia
            );
            
            data.PosicaoIlha = posicaoIlha;
            
            CriarPlataformaIlha(posicaoIlha);
            
            Vector3 posicaoJogador = posicaoIlha + Vector3.up * 2f;
            player.Teleport(posicaoJogador);
        }
        
        private void CriarPlataformaIlha(Vector3 centro)
        {
            float tamanho = configData.TamanhoIlha;
            int blocosPorLado = 20;
            float tamanhoBloco = tamanho / blocosPorLado;
            
            for (int x = 0; x < blocosPorLado; x++)
            {
                for (int z = 0; z < blocosPorLado; z++)
                {
                    Vector3 posicaoBloco = new Vector3(
                        centro.x - tamanho/2 + (x * tamanhoBloco) + tamanhoBloco/2,
                        centro.y - 0.5f,
                        centro.z - tamanho/2 + (z * tamanhoBloco) + tamanhoBloco/2
                    );
                    
                    var buildingBlock = GameManager.server.CreateEntity("assets/bundled/prefabs/building/block.stone.prefab", posicaoBloco) as BuildingBlock;
                    if (buildingBlock != null)
                    {
                        buildingBlock.Spawn();
                        buildingBlock.SetGrade(BuildingGrade.Enum.Stone);
                        buildingBlock.SetHealthToMax();
                    }
                }
            }
        }
        
        private void RemoverIlha(ulong userID)
        {
            if (!playerData.ContainsKey(userID)) return;
            
            var data = playerData[userID];
            if (data.PosicaoIlha == Vector3.zero) return;
            
            float tamanho = configData.TamanhoIlha;
            var entities = UnityEngine.Object.FindObjectsOfType<BuildingBlock>();
            
            foreach (var entity in entities)
            {
                if (entity == null || !entity.IsValid()) continue;
                
                Vector3 pos = entity.transform.position;
                float distancia = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(data.PosicaoIlha.x, 0, data.PosicaoIlha.z));
                
                if (distancia <= tamanho / 2 + 10f)
                {
                    entity.Kill();
                }
            }
            
            if (ilhasAtivas.ContainsKey(data.PosicaoIlha))
            {
                ilhasAtivas.Remove(data.PosicaoIlha);
            }
        }
        
        private void AdicionarJogadorNaIlha(ulong userID, Vector3 posicaoIlha)
        {
            if (!ilhasAtivas.ContainsKey(posicaoIlha))
            {
                ilhasAtivas[posicaoIlha] = new IlhaData { Posicao = posicaoIlha };
            }
            
            if (!ilhasAtivas[posicaoIlha].PlayersNaIlha.Contains(userID))
            {
                ilhasAtivas[posicaoIlha].PlayersNaIlha.Add(userID);
            }
        }
        
        private void RemoverJogadorDaIlha(ulong userID, Vector3 posicaoIlha)
        {
            if (ilhasAtivas.ContainsKey(posicaoIlha))
            {
                ilhasAtivas[posicaoIlha].PlayersNaIlha.Remove(userID);
                if (ilhasAtivas[posicaoIlha].PlayersNaIlha.Count == 0)
                {
                    ilhasAtivas.Remove(posicaoIlha);
                }
            }
            
            if (ilhasBots.ContainsKey(posicaoIlha))
            {
                ilhasBots[posicaoIlha].PlayersNaIlha.Remove(userID);
            }
        }
        
        #endregion
        
        #region Sistema de Bots
        
        private void CriarIlhasBots()
        {
            // Criar 3 ilhas permanentes com bots (uma para cada dificuldade)
            string[] dificuldades = { "facil", "medio", "dificil" };
            
            for (int i = 0; i < dificuldades.Length; i++)
            {
                float angulo = (i * 120f) * Mathf.Deg2Rad;
                float distancia = 2000f;
                
                Vector3 posicaoIlha = new Vector3(
                    Mathf.Cos(angulo) * distancia,
                    configData.AlturaIlha,
                    Mathf.Sin(angulo) * distancia
                );
                
                CriarPlataformaIlha(posicaoIlha);
                
                ilhasBots[posicaoIlha] = new IlhaBotData
                {
                    Posicao = posicaoIlha,
                    Dificuldade = dificuldades[i]
                };
                
                CriarBotsNaIlha(posicaoIlha, dificuldades[i]);
            }
        }
        
        private Vector3 EncontrarIlhaBots(string dificuldade)
        {
            foreach (var ilha in ilhasBots.Values)
            {
                if (ilha.Dificuldade == dificuldade)
                {
                    return ilha.Posicao;
                }
            }
            return Vector3.zero;
        }
        
        private void CriarBotsNaIlha(Vector3 posicaoIlha, string dificuldade)
        {
            if (!ilhasBots.ContainsKey(posicaoIlha)) return;
            
            int quantidadeBots = 0;
            switch (dificuldade)
            {
                case "facil":
                    quantidadeBots = configData.QuantidadeBotsFacil;
                    break;
                case "medio":
                    quantidadeBots = configData.QuantidadeBotsMedio;
                    break;
                case "dificil":
                    quantidadeBots = configData.QuantidadeBotsDificil;
                    break;
            }
            
            float tamanho = configData.TamanhoIlha;
            float raio = tamanho / 3f;
            
            for (int i = 0; i < quantidadeBots; i++)
            {
                float angulo = (i * 360f / quantidadeBots) * Mathf.Deg2Rad;
                Vector3 posicaoBot = new Vector3(
                    posicaoIlha.x + Mathf.Cos(angulo) * raio,
                    posicaoIlha.y + 1f,
                    posicaoIlha.z + Mathf.Sin(angulo) * raio
                );
                
                CriarBot(posicaoBot, dificuldade, posicaoIlha);
            }
        }
        
        private void CriarBot(Vector3 posicao, string dificuldade, Vector3 posicaoIlha)
        {
            // Criar NPC usando a API do Rust
            var npc = GameManager.server.CreateEntity("assets/rust.ai/agents/npcplayer/npcplayer.prefab", posicao) as BaseNpc;
            if (npc == null) return;
            
            npc.Spawn();
            npc.InitializeHealth(npc.startHealth, npc.startHealth);
            
            // Configurar bot baseado na dificuldade
            ConfigurarBot(npc, dificuldade);
            
            // Adicionar à lista de bots da ilha
            if (ilhasBots.ContainsKey(posicaoIlha))
            {
                ilhasBots[posicaoIlha].Bots.Add(npc);
            }
            
            // Iniciar AI do bot
            timer.Once(1f, () => IniciarAIBot(npc, dificuldade, posicaoIlha));
        }
        
        private void ConfigurarBot(BaseNpc bot, string dificuldade)
        {
            // Dar armas e equipamentos ao bot
            var arma = ItemManager.CreateByName("rifle.ak");
            if (arma != null && bot.inventory != null)
            {
                bot.inventory.GiveItem(arma);
                bot.UpdateActiveItem(arma.uid);
            }
            
            var municao = ItemManager.CreateByName("ammo.rifle", 1000);
            if (municao != null && bot.inventory != null)
            {
                bot.inventory.GiveItem(municao);
            }
            
            // Configurar stats baseado na dificuldade
            switch (dificuldade)
            {
                case "facil":
                    bot.InitializeHealth(50f, 50f);
                    break;
                case "medio":
                    bot.InitializeHealth(100f, 100f);
                    break;
                case "dificil":
                    bot.InitializeHealth(150f, 150f);
                    break;
            }
        }
        
        private void IniciarAIBot(BaseNpc bot, string dificuldade, Vector3 posicaoIlha)
        {
            if (bot == null || bot.IsDead) return;
            
            // Encontrar alvo (jogador mais próximo)
            BasePlayer alvo = EncontrarAlvoMaisProximo(bot, posicaoIlha);
            
            if (alvo != null)
            {
                // Mover em direção ao alvo
                MoverBotParaAlvo(bot, alvo, dificuldade);
                
                // Atirar no alvo
                AtirarBotNoAlvo(bot, alvo, dificuldade);
            }
            else
            {
                // Patrulhar
                PatrulharBot(bot, posicaoIlha, dificuldade);
            }
            
            // Repetir após delay baseado na dificuldade
            float delay = dificuldade == "facil" ? 2f : (dificuldade == "medio" ? 1f : 0.5f);
            timer.Once(delay, () => IniciarAIBot(bot, dificuldade, posicaoIlha));
        }
        
        private BasePlayer EncontrarAlvoMaisProximo(BaseNpc bot, Vector3 posicaoIlha)
        {
            BasePlayer alvoMaisProximo = null;
            float distanciaMinima = float.MaxValue;
            float raioBusca = configData.TamanhoIlha;
            
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || player.IsDead) continue;
                
                if (!playerData.ContainsKey(player.userID)) continue;
                
                var data = playerData[player.userID];
                if (!data.EmAreaPvP || data.PosicaoIlha != posicaoIlha) continue;
                
                float distancia = Vector3.Distance(bot.transform.position, player.transform.position);
                if (distancia < distanciaMinima && distancia <= raioBusca)
                {
                    distanciaMinima = distancia;
                    alvoMaisProximo = player;
                }
            }
            
            return alvoMaisProximo;
        }
        
        private void MoverBotParaAlvo(BaseNpc bot, BasePlayer alvo, string dificuldade)
        {
            if (bot == null || alvo == null || bot.IsDead) return;
            
            Vector3 direcao = (alvo.transform.position - bot.transform.position).normalized;
            float velocidade = dificuldade == "facil" ? 2f : (dificuldade == "medio" ? 3f : 4f);
            
            // Usar NavMeshAgent se disponível
            var navAgent = bot.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.SetDestination(alvo.transform.position);
                navAgent.speed = velocidade;
            }
            else
            {
                // Movimento simples
                bot.transform.position += direcao * velocidade * Time.deltaTime;
            }
        }
        
        private void AtirarBotNoAlvo(BaseNpc bot, BasePlayer alvo, string dificuldade)
        {
            if (bot == null || alvo == null || bot.IsDead) return;
            
            float distancia = Vector3.Distance(bot.transform.position, alvo.transform.position);
            float distanciaMaxima = dificuldade == "facil" ? 30f : (dificuldade == "medio" ? 50f : 70f);
            
            if (distancia > distanciaMaxima) return;
            
            // Olhar para o alvo
            Vector3 direcao = (alvo.transform.position - bot.transform.position).normalized;
            bot.transform.rotation = Quaternion.LookRotation(direcao);
            
            // Atirar
            var heldEntity = bot.GetHeldEntity() as BaseProjectile;
            if (heldEntity != null)
            {
                float precisao = dificuldade == "facil" ? 0.5f : (dificuldade == "medio" ? 0.7f : 0.9f);
                
                if (UnityEngine.Random.value < precisao)
                {
                    heldEntity.ServerUse();
                }
            }
        }
        
        private void PatrulharBot(BaseNpc bot, Vector3 posicaoIlha, string dificuldade)
        {
            if (bot == null || bot.IsDead) return;
            
            float raio = configData.TamanhoIlha / 3f;
            Vector3 posicaoAleatoria = posicaoIlha + new Vector3(
                UnityEngine.Random.Range(-raio, raio),
                0,
                UnityEngine.Random.Range(-raio, raio)
            );
            
            var navAgent = bot.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.SetDestination(posicaoAleatoria);
            }
        }
        
        #endregion
        
        #region Console Commands
        
        [ConsoleCommand("pvpbots.close")]
        private void CmdBotsClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            CuiHelper.DestroyUi(player, "BotsMenuPanel");
        }
        
        #endregion
    }
}
