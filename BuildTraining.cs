using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;  // ← ADICIONADO PARA CuiHelper

namespace Oxide.Plugins
{
    [Info("Build Training", "Seu Nome", "1.0.0")]
    [Description("Plugin de treino de construção de bases com sistema de ilhas")]
    public class BuildTraining : RustPlugin
    {
        #region Configuração
        
        private ConfigData configData;
        
        private class ConfigData
        {
            public bool Habilitado { get; set; } = true;
            public float TamanhoIlha { get; set; } = 100f;
            public float DistanciaEntreIlhas { get; set; } = 500f;
            public float AlturaIlha { get; set; } = 50f;
            public int MaterialInicialMadeira { get; set; } = 10000;
            public int MaterialInicialPedra { get; set; } = 10000;
            public int MaterialInicialMetal { get; set; } = 5000;
            public int MaterialInicialFragmentos { get; set; } = 1000;
            public List<BuildPreset> BuildsDisponiveis { get; set; } = new List<BuildPreset>
            {
                new BuildPreset { Nome = "Base Pequena", Descricao = "Base compacta para iniciantes" },
                new BuildPreset { Nome = "Base Média", Descricao = "Base intermediária" },
                new BuildPreset { Nome = "Base Grande", Descricao = "Base avançada" },
                new BuildPreset { Nome = "Base Customizada", Descricao = "Construa sua própria base" }
            };
        }
        
        private class BuildPreset
        {
            public string Nome { get; set; }
            public string Descricao { get; set; }
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
        
        private Dictionary<ulong, PlayerBuildData> playerData = new Dictionary<ulong, PlayerBuildData>();
        
        private class PlayerBuildData
        {
            public Vector3 PosicaoIlha { get; set; }
            public bool EmIlha { get; set; } = false;
            public Vector3 PosicaoOriginal { get; set; }
            public Dictionary<string, int> MateriaisGastos { get; set; } = new Dictionary<string, int>();
            public string BuildSelecionado { get; set; } = "";
        }
        
        #endregion
        
        #region Hooks
        
        private void Init()
        {
            if (!configData.Habilitado)
            {
                PrintWarning("Plugin Build Training desabilitado na configuração!");
                return;
            }
            
            cmd.AddChatCommand("build", this, nameof(CmdBuild));
            cmd.AddChatCommand("sairilha", this, nameof(CmdSairIlha));
            cmd.AddChatCommand("recursos", this, nameof(CmdRecursos));
            cmd.AddChatCommand("materiais", this, nameof(CmdMateriais));
            
            PrintWarning("Plugin Build Training inicializado!");
        }
        
        private void OnServerInitialized()
        {
            PrintWarning($"Plugin Build Training {Version} está ativo.");
        }
        
        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player != null && playerData.ContainsKey(player.userID))
            {
                var data = playerData[player.userID];
                if (data.EmIlha)
                {
                    RemoverIlha(player.userID);
                }
            }
        }
        
        private void OnEntityBuilt(Planner plan, GameObject go)
        {
            var player = plan.GetOwnerPlayer();
            if (player == null || !playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            if (!data.EmIlha) return;
            
            var buildingBlock = go.GetComponent<BuildingBlock>();
            if (buildingBlock == null) return;
            
            var grade = buildingBlock.grade;
            string materialNome = "";
            int quantidade = 0;
            
            switch (grade)
            {
                case BuildingGrade.Enum.Twigs:
                    materialNome = "Madeira";
                    quantidade = 10;
                    break;
                case BuildingGrade.Enum.Wood:
                    materialNome = "Madeira";
                    quantidade = 100;
                    break;
                case BuildingGrade.Enum.Stone:
                    materialNome = "Pedra";
                    quantidade = 200;
                    break;
                case BuildingGrade.Enum.Metal:
                    materialNome = "Metal";
                    quantidade = 300;
                    break;
                case BuildingGrade.Enum.TopTier:
                    materialNome = "Fragmentos";
                    quantidade = 500;
                    break;
            }
            
            if (!string.IsNullOrEmpty(materialNome))
            {
                if (!data.MateriaisGastos.ContainsKey(materialNome))
                {
                    data.MateriaisGastos[materialNome] = 0;
                }
                data.MateriaisGastos[materialNome] += quantidade;
                
                AtualizarUIMateriais(player);
            }
        }
        
        #endregion
        
        #region Comandos
        
        [ChatCommand("build")]
        private void CmdBuild(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerBuildData();
            }
            
            var data = playerData[player.userID];
            
            if (data.EmIlha)
            {
                player.ChatMessage("<color=red>Você já está em uma ilha! Use /sairilha para sair.</color>");
                return;
            }
            
            MostrarUISelecaoBuild(player);
        }
        
        [ChatCommand("sairilha")]
        private void CmdSairIlha(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            if (!data.EmIlha)
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            RemoverIlha(player.userID);
            player.Teleport(data.PosicaoOriginal);
            data.EmIlha = false;
            data.PosicaoIlha = Vector3.zero;
            data.MateriaisGastos.Clear();
            
            player.ChatMessage("<color=green>Você saiu da ilha de construção!</color>");
            CuiHelper.DestroyUi(player, "MateriaisUI");
        }
        
        [ChatCommand("recursos")]
        private void CmdRecursos(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            if (!data.EmIlha)
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            DarRecursos(player);
            player.ChatMessage("<color=green>Recursos adicionados ao seu inventário!</color>");
        }
        
        [ChatCommand("materiais")]
        private void CmdMateriais(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            
            if (!playerData.ContainsKey(player.userID))
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            var data = playerData[player.userID];
            if (!data.EmIlha)
            {
                player.ChatMessage("<color=red>Você não está em uma ilha!</color>");
                return;
            }
            
            MostrarUIMateriais(player);
        }
        
        #endregion
        
        #region UI
        
        private void MostrarUISelecaoBuild(BasePlayer player)
        {
            var container = new CuiElementContainer();
            string panel = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.95" },
                RectTransform = { AnchorMin = "0.2 0.2", AnchorMax = "0.8 0.8" },
                CursorEnabled = true
            }, "Overlay", "BuildSelectionPanel");
            
            container.Add(new CuiLabel
            {
                Text = { Text = "SELECIONE SEU BUILD", FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 1" }
            }, panel);
            
            float buttonHeight = 0.12f;
            float spacing = 0.02f;
            float startY = 0.7f;
            
            for (int i = 0; i < configData.BuildsDisponiveis.Count; i++)
            {
                var build = configData.BuildsDisponiveis[i];
                float yMin = startY - (i * (buttonHeight + spacing));
                float yMax = yMin + buttonHeight;
                
                string buttonName = $"BuildButton_{i}";
                container.Add(new CuiButton
                {
                    Button = { Command = $"build.select {i}", Color = "0.2 0.4 0.8 1" },
                    RectTransform = { AnchorMin = $"0.1 {yMin}", AnchorMax = $"0.9 {yMax}" },
                    Text = { Text = $"{build.Nome}\n{build.Descricao}", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, panel, buttonName);
            }
            
            container.Add(new CuiButton
            {
                Button = { Command = "build.close", Color = "0.8 0.2 0.2 1" },
                RectTransform = { AnchorMin = "0.3 0.05", AnchorMax = "0.7 0.15" },
                Text = { Text = "FECHAR", FontSize = 18, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, panel);
            
            CuiHelper.AddUi(player, container);
        }
        
        private void MostrarUIMateriais(BasePlayer player)
        {
            if (!playerData.ContainsKey(player.userID)) return;
            
            var data = playerData[player.userID];
            
            CuiHelper.DestroyUi(player, "MateriaisUI");
            
            var container = new CuiElementContainer();
            string panel = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.9" },
                RectTransform = { AnchorMin = "0.75 0.7", AnchorMax = "0.98 0.98" },
                CursorEnabled = false
            }, "Overlay", "MateriaisUI");
            
            container.Add(new CuiLabel
            {
                Text = { Text = "MATERIAIS GASTOS", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 1" }
            }, panel);
            
            float lineHeight = 0.12f;
            float startY = 0.7f;
            int index = 0;
            
            foreach (var material in data.MateriaisGastos.OrderByDescending(x => x.Value))
            {
                float yMin = startY - (index * lineHeight);
                float yMax = yMin + lineHeight;
                
                string color = GetMaterialColor(material.Key);
                container.Add(new CuiLabel
                {
                    Text = { Text = $"{material.Key}: {material.Value:N0}", FontSize = 14, Align = TextAnchor.MiddleLeft, Color = color },
                    RectTransform = { AnchorMin = $"0.05 {yMin}", AnchorMax = $"0.95 {yMax}" }
                }, panel);
                
                index++;
            }
            
            if (data.MateriaisGastos.Count == 0)
            {
                container.Add(new CuiLabel
                {
                    Text = { Text = "Nenhum material gasto ainda", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "0.7 0.7 0.7 1" },
                    RectTransform = { AnchorMin = "0 0.3", AnchorMax = "1 0.7" }
                }, panel);
            }
            
            CuiHelper.AddUi(player, container);
        }
        
        private void AtualizarUIMateriais(BasePlayer player)
        {
            MostrarUIMateriais(player);
        }
        
        private string GetMaterialColor(string material)
        {
            switch (material.ToLower())
            {
                case "madeira":
                    return "0.8 0.6 0.4 1";
                case "pedra":
                    return "0.7 0.7 0.7 1";
                case "metal":
                    return "0.5 0.5 0.8 1";
                case "fragmentos":
                    return "0.9 0.9 0.3 1";
                default:
                    return "1 1 1 1";
            }
        }
        
        #endregion
        
        #region Sistema de Ilhas
        
        private void CriarIlha(BasePlayer player, string buildNome)
        {
            if (!playerData.ContainsKey(player.userID))
            {
                playerData[player.userID] = new PlayerBuildData();
            }
            
            var data = playerData[player.userID];
            data.PosicaoOriginal = player.transform.position;
            
            // Calcular posição da ilha baseada no ID do jogador
            int ilhaIndex = (int)(player.userID % 1000);
            float angulo = (ilhaIndex * 15f) * Mathf.Deg2Rad;
            float distancia = configData.DistanciaEntreIlhas + (ilhaIndex * 10f);
            
            Vector3 posicaoIlha = new Vector3(
                Mathf.Cos(angulo) * distancia,
                configData.AlturaIlha,
                Mathf.Sin(angulo) * distancia
            );
            
            data.PosicaoIlha = posicaoIlha;
            data.EmIlha = true;
            data.BuildSelecionado = buildNome;
            data.MateriaisGastos.Clear();
            
            // Criar plataforma da ilha
            CriarPlataformaIlha(posicaoIlha);
            
            // Teleportar jogador
            Vector3 posicaoJogador = posicaoIlha + Vector3.up * 2f;
            player.Teleport(posicaoJogador);
            
            player.ChatMessage($"<color=green>Você foi teleportado para sua ilha de construção!</color>");
            player.ChatMessage($"<color=yellow>Build selecionado: {buildNome}</color>");
            player.ChatMessage($"<color=yellow>Use /recursos para pegar materiais</color>");
            player.ChatMessage($"<color=yellow>Use /materiais para ver materiais gastos</color>");
            player.ChatMessage($"<color=yellow>Use /sairilha para sair da ilha</color>");
            
            timer.Once(1f, () => {
                DarRecursos(player);
                MostrarUIMateriais(player);
            });
        }
        
        private void CriarPlataformaIlha(Vector3 centro)
        {
            // Criar uma plataforma de pedra para a ilha
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
            
            // Remover todos os blocos na área da ilha
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
        }
        
        #endregion
        
        #region Recursos
        
        private void DarRecursos(BasePlayer player)
        {
            if (player == null || player.inventory == null) return;
            
            // Madeira
            var madeira = ItemManager.CreateByName("wood", configData.MaterialInicialMadeira);
            if (madeira != null)
            {
                player.inventory.GiveItem(madeira);
            }
            
            // Pedra
            var pedra = ItemManager.CreateByName("stones", configData.MaterialInicialPedra);
            if (pedra != null)
            {
                player.inventory.GiveItem(pedra);
            }
            
            // Metal
            var metal = ItemManager.CreateByName("metal.fragments", configData.MaterialInicialMetal);
            if (metal != null)
            {
                player.inventory.GiveItem(metal);
            }
            
            // Fragmentos
            var fragmentos = ItemManager.CreateByName("scrap", configData.MaterialInicialFragmentos);
            if (fragmentos != null)
            {
                player.inventory.GiveItem(fragmentos);
            }
            
            // Adicionar alguns itens úteis
            var martelo = ItemManager.CreateByName("hammer");
            if (martelo != null)
            {
                player.inventory.GiveItem(martelo);
            }
            
            var plano = ItemManager.CreateByName("building.planner");
            if (plano != null)
            {
                player.inventory.GiveItem(plano);
            }
        }
        
        #endregion
        
        #region Console Commands
        
        [ConsoleCommand("build.select")]
        private void CmdBuildSelect(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            if (arg.Args == null || arg.Args.Length == 0)
            {
                return;
            }
            
            // CORRIGIDO: Declarar buildIndex ANTES do TryParse (C# 6.0)
            int buildIndex;
            if (!int.TryParse(arg.Args[0], out buildIndex))
            {
                return;
            }
            
            if (buildIndex < 0 || buildIndex >= configData.BuildsDisponiveis.Count)
            {
                return;
            }
            
            var build = configData.BuildsDisponiveis[buildIndex];
            CuiHelper.DestroyUi(player, "BuildSelectionPanel");
            CriarIlha(player, build.Nome);
        }
        
        [ConsoleCommand("build.close")]
        private void CmdBuildClose(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            CuiHelper.DestroyUi(player, "BuildSelectionPanel");
        }
        
        #endregion
    }
}
