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
    [Info("Item Skins", "YourName", "1.3.0")]
    [Description("Sistema completo de skins com UI visual e imagens")]
    public class ItemSkins : RustPlugin
    {
        #region Plugins Externos
        
        [PluginReference]
        private Plugin ImageLibrary;
        
        #endregion
        
        #region Configuração
        
        private ConfigData configData;
        
        private class ConfigData
        {
            [JsonProperty("Habilitado")]
            public bool Habilitado { get; set; } = true;
            
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
            
            [JsonProperty("Skins por página")]
            public int SkinsPorPagina { get; set; } = 48;
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
        private Dictionary<ulong, int> playerCurrentPage = new Dictionary<ulong, int>();
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
            CarregarSkinsPersonalizadas();
            Puts($"Carregadas {itemSkins.Count} categorias de itens com skins!");
            
            int totalSkins = 0;
            foreach (var skins in itemSkins.Values)
            {
                totalSkins += skins.Count;
            }
            Puts($"Total de {totalSkins} skins disponíveis!");
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
                if (playerCurrentPage.ContainsKey(player.userID))
                {
                    playerCurrentPage.Remove(player.userID);
                }
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
                MostrarUISkinsItem(player, item, 0);
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
            
            MostrarUISkinsItem(player, item, 0);
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
                MostrarFavoritos(player);
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
        
        private void CarregarSkinsPersonalizadas()
        {
            itemSkins.Clear();
            
            // Adicionar todas as skins fornecidas pelo usuário
            itemSkins["shoes.boots"] = new List<ulong> { 3487239287, 3484649356, 3485932851, 3486329856, 3044786385, 2706111487, 2997732809, 3332536449, 3009110915, 3390509576, 2589447719, 2946627120, 2483680748, 2882151877, 3328005555, 3235752720, 3317205294, 2529291753, 3097589772, 3066504148, 3479699869, 2949281691, 2991572182, 3009110212, 2752873720, 883374703, 2510093391, 2090776132, 2575506021, 3088669078, 3164459704, 3433579334, 2833767826, 3472139629, 2816776847, 2304198263, 2799638251, 2836455608, 2932448101, 2688629448, 3124951730, 2894567050, 2661866565, 2980941295, 1703220348, 1111680681, 2744190110, 2397371398, 1229544227, 3472458820, 2497055510, 3438187163, 3351330888, 3274870534, 3251246600, 613481881, 636286960, 869090082, 920390242, 1100926907, 1441308562, 1839313604, 1864539854, 1915397286, 1196740980, 1915955573, 2730109230, 2903972151, 2504102457, 1995685684, 3047756539, 3440509335, 3318426834, 2982282299, 2792888286, 2840291890, 2075527039, 944997041, 2199934989, 2863609297, 2338893326, 2410871443, 2496064553, 1260559517, 1822096375, 2464929870, 2463863571, 2462860331, 2461996389, 2461918934, 2461911046, 2461087503, 2458705659, 2458658506, 2458627387, 2458608294, 2458577081, 2455992473, 2455652822, 2450218370, 2447137992, 2445802829, 2441613214, 2436389679, 2424796441, 2375042259, 2698787015, 2703364234, 803927656, 2363008397, 2361574127, 2415240702, 2360921215, 2361576886, 2403112229, 2675531117, 2552314771, 2552310054, 2530083588, 2570215282, 2760435199, 898352612, 2790755098, 2179993806, 2050038638, 1158967113, 2350298338, 2498732574, 2655116999, 2570215748, 1645414196, 2618817211, 2581371666, 2537679237, 855424554, 784559403, 809586899, 882570089, 2360810326, 1920242394, 2138787134, 2143270123, 1088000573, 809972564, 956207525, 1132775378, 1427198029, 1125251812, 1210771348, 803424172, 953299457, 1334218321, 1425280190, 824660476, 823382589, 934289804, 962503020, 1084392788, 961096730, 2454376365, 899942107, 919261524, 1106548545, 2948676369, 1395755190, 2365893355, 2365760945, 2364719784, 2364703171, 2362366589, 2362275451, 1323486001, 2361565310, 2360936392, 2360737706, 2357383966, 2345769400, 2400968847, 2400879286, 2403545451, 2402281710, 2405742483, 3199681256, 1234675825, 2006541391, 2936561559, 1428936568, 1192621630, 2830476912, 1464542400, 971729488, 2546014415, 2507113985, 2546009776, 2380731293, 2685754112, 2677555364, 2753961785, 2613289590, 2490448596, 2470695159, 2383808458, 852861153, 1066488938, 2379136614, 793151728, 2401312480, 2006613455, 874792536, 1227235647, 1623178618, 749127039, 916448999, 1291665415, 1178208248, 1113477231, 926826859, 1197742619, 1170428844, 1101343374, 1134280832, 826626541, 1172088869, 1313773431, 906799290, 823562988, 827079596, 822368710, 825403970, 627529165, 1093790510, 1134388193, 1645719721, 1467522434, 849640105, 833720677, 827401839, 824095656, 816355972, 1088899968, 971302078, 1135820993, 1095595792, 1644270941, 877753776, 971290350, 1526996873, 906611150, 814118383, 2093192491, 1508390281, 842367330, 1092200142, 1367606093, 845328750, 844477684, 1080534016, 1127871007, 869007492, 1080992702, 847577255, 1083322699, 2289331474, 1264775379, 818315741, 1259322673, 850241381, 1577821136, 1275881929, 901359832, 845593835, 763971519, 875555282, 1188606232, 807969408, 809524164, 824103000, 751622671, 897843549, 2023914138, 911186114, 2255215710, 784769996, 1829430494, 1909457079, 2281466765, 2391743981, 2066609589, 2388770109, 2165075669, 2100888662, 1117128014, 2186835886, 1240569776, 1218539775, 1797202041, 1558498391, 1557277341, 1558550451, 1554036010, 1170010955, 819211835, 842175164, 811633396, 796019171, 1424368814, 868753471, 842175932, 1259338450, 836949176, 811809821, 959715677, 848071484, 843699236, 838711170, 1264205245, 833645549, 1259336999, 1230114411, 838725990, 865011139, 1300771262, 860848252, 856474410, 934146907, 837807357, 1070260501, 1113572837, 1405445971, 848894996, 818896250, 1107905926, 2713643378, 3407060118, 2476034973, 2330162397, 2673890130, 873451516, 2593285922, 2424028673, 1338888495, 2266184366, 2957314019, 1338135414, 2543385320, 918711787, 2621904702, 2855381013, 2394441130, 2390044937, 2382084100, 2381424763, 2381308033, 2381049110, 2379425682, 2378164371, 2372327802, 2371137028, 2370434734, 898315666, 2957774272, 3150694821, 2006702685, 2009426933, 2945381045 };
            
            itemSkins["coffeecan.helmet"] = new List<ulong> { 3487235363, 3484176678, 3484446181, 3483793514, 3485300682, 3485356101, 3486327166, 2696378175, 2862346208, 2964104594, 2991835101, 2979241222, 2722821835, 2799237463, 3437952219, 2942984470, 3352739473, 3200869471, 3239594798, 3428339247, 3088345444, 3415728548, 3446489533, 3472579808, 3283259479, 3139561244, 3472125086, 3445543108, 2985414025, 2460303323, 3199386002, 2495422757, 2503956851, 2496517898, 948491992, 1804649832, 2120618167, 2865359782, 2570227850, 2966721462, 3159713751, 3295005539, 3367975601, 3045043901, 3223770718, 2855866294, 2803024592, 3001653412, 3256066116, 2199783358, 3323219530, 3323415617, 2076260082, 2792860812, 2823739686, 2942986068, 3067103821, 3067106786, 1557891153, 2601517551, 3034164351, 2562697065, 2715609678, 3457338462, 3458364043, 3457182166, 1743856800, 1349946203, 2397368205, 2846422419, 3342769510, 3468267721, 2462621514, 3442437861, 3326567792, 3260694842, 1129809202, 1400824309, 1349166206, 1248435433, 1251411840, 938020581, 891592450, 914060966, 784910461, 806212029, 843676357, 919595880, 955675586, 970583835, 1130589746, 1174375607, 974321420, 1442169133, 1539575334, 1539650632, 1759479029, 1797478191, 1740061403, 1865208631, 1894381558, 1906527802, 1944168755, 2943400927, 2497169921, 2875798335, 3427361769, 2865299082, 3323534570, 3356574669, 3402447254, 2855632841, 2782671260, 2320222274, 2350097716, 1438088592, 1445131741, 1269589560, 2463112058, 891241596, 2320293094, 966099933, 2497223189, 1935356290, 2562240855, 1987848469, 811030719, 1974807032, 1388417865, 1121458604, 2252268774, 2051201729, 2112768279, 2275795658, 2646384466, 2551769961, 2543090576, 2491406956, 814098474, 848645884, 854460770, 2005953795, 1727561127, 785627245, 810084426, 1238664378, 810467215, 810423506, 796129021, 810421083, 792753762, 879380450, 1290207240, 1474456258, 1367271049, 1421293757, 823725912, 1088543909, 844909678, 1277653462, 895586218, 1605589318, 827185235, 812293509, 1202978872, 1151227603, 1154453278, 1826193299, 3201132879, 1178140242, 954112263, 866322538, 817375411, 894200783, 957705235, 840476654, 949554654, 1391003757, 834460563, 815140405, 1102661570, 812224252, 842135257, 881159727, 812361131, 860003666, 813312307, 956819150, 1138289539, 817473975, 856942764, 884751189, 1103199253, 827146872, 845743182, 814198295, 884788545, 813685003, 883379813, 816986525, 813322239, 1199447873, 927395637, 1231602228, 813249543, 1127398461, 833831330, 917701689, 842204585, 840885377, 817075857, 815912801, 835966137, 847813832, 848086003, 815222900, 883329864, 868837709, 1108963876, 861487358, 848015134, 854428250, 838097550, 1472513928, 829558641, 846981679, 870070455, 856389472, 873470337, 844315152, 818623972, 1347405993, 1533536355, 842182395, 815708141, 886042078, 888388545, 884741244, 814481999, 841211564, 835188718, 2366552401, 2365892231, 2365887691, 2365757122, 2364717612, 2364699578, 2362784473, 2362272282, 1320679470, 2359695475, 2351112464, 2329533631, 2403509781, 2411694697, 2417758938, 1396123701, 2827141289, 1687183247, 2296710564, 2142393198, 947015606, 1666434377, 1476941984, 1915442448, 1811523301, 2051690872, 2001044460, 1932198107, 1405851647, 1973542558, 2713576642, 1852240690, 2538546441, 2538273356, 2366291607, 1342122459, 1624104393, 2593282476, 2147200135, 2454442861, 2394600997, 2381038653, 2380594286, 2378975473, 2378083466, 2373702942, 2373049965, 2371106008, 2577066506, 2782719146, 2929407406, 2936058835, 2936421445 };
            
            itemSkins["coffeecan.helmet"] = itemSkins["coffeecan.helmet"] ?? new List<ulong>();
            
            // Continuar com os outros itens...
            CarregarSkinsRestantes();
            
            Puts($"Sistema de skins carregado: {itemSkins.Count} tipos de itens");
        }
        
        private void CarregarSkinsRestantes()
        {
            // Carregar skins do arquivo JSON externo
            string filePath = $"{Interface.Oxide.DataDirectory}{System.IO.Path.DirectorySeparatorChar}ItemSkins_Database.json";
            
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string jsonContent = System.IO.File.ReadAllText(filePath);
                    var skinsDatabase = JsonConvert.DeserializeObject<Dictionary<string, List<ulong>>>(jsonContent);
                    
                    if (skinsDatabase != null)
                    {
                        int itensCarregados = 0;
                        int skinsCarregadas = 0;
                        
                        foreach (var item in skinsDatabase)
                        {
                            if (!itemSkins.ContainsKey(item.Key))
                            {
                                itemSkins[item.Key] = item.Value;
                                itensCarregados++;
                                skinsCarregadas += item.Value.Count;
                            }
                            else
                            {
                                // Mesclar skins se já existirem
                                foreach (var skinId in item.Value)
                                {
                                    if (!itemSkins[item.Key].Contains(skinId))
                                    {
                                        itemSkins[item.Key].Add(skinId);
                                        skinsCarregadas++;
                                    }
                                }
                            }
                        }
                        
                        Puts($"✅ Carregadas {skinsCarregadas} skins de {itensCarregados} itens do arquivo JSON!");
                    }
                }
                catch (System.Exception ex)
                {
                    PrintError($"Erro ao carregar skins do arquivo JSON: {ex.Message}");
                }
            }
            else
            {
                PrintWarning($"Arquivo de skins não encontrado: {filePath}");
                PrintWarning("O plugin funcionará apenas com as skins já carregadas no código.");
            }
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
        
        private void MostrarFavoritos(BasePlayer player)
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
        
        #endregion
        
        #region UI
        
        private void MostrarUISkinsItem(BasePlayer player, Item item, int pagina)
        {
            if (!itemSkins.ContainsKey(item.info.shortname))
            {
                player.ChatMessage($"<color=red>{item.info.displayName.english} não possui skins disponíveis!</color>");
                player.ChatMessage($"<color=yellow>Itens disponíveis: AK47, Thompson, LR-300, Hoodie, Calças, Botas, e mais!</color>");
                return;
            }
            
            DestruirUI(player);
            
            var skins = itemSkins[item.info.shortname];
            var container = new CuiElementContainer();
            
            // ====== PAINEL PRINCIPAL ======
            string mainPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.08 0.08 0.08 0.98" },
                RectTransform = { AnchorMin = "0.15 0.1", AnchorMax = "0.85 0.9" },
                CursorEnabled = true
            }, "Overlay", "SkinsPanel");
            
            // ====== HEADER (BARRA SUPERIOR) ======
            string headerPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.2 0.4 0.7 1" },
                RectTransform = { AnchorMin = "0 0.92", AnchorMax = "1 1" }
            }, mainPanel);
            
            // Título no header
            container.Add(new CuiLabel
            {
                Text = { 
                    Text = $"🎨 SKINS - {item.info.displayName.english.ToUpper()}", 
                    FontSize = 22, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, headerPanel);
            
            // ====== BARRA DE INFORMAÇÕES ======
            // Calcular paginação
            int skinsPerPage = configData.SkinsPorPagina;
            int totalPaginas = Mathf.CeilToInt((float)skins.Count / skinsPerPage);
            pagina = Mathf.Clamp(pagina, 0, totalPaginas - 1);
            
            playerCurrentPage[player.userID] = pagina;
            
            string infoPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.15 0.15 0.15 1" },
                RectTransform = { AnchorMin = "0 0.87", AnchorMax = "1 0.91" }
            }, mainPanel);
            
            container.Add(new CuiLabel
            {
                Text = { 
                    Text = $"📊 {skins.Count} skins disponíveis  |  📄 Página {pagina + 1}/{totalPaginas}  |  👆 Clique para aplicar", 
                    FontSize = 14, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.8 0.9 1 1"
                },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, infoPanel);
            
            // ====== ÁREA DE SKINS (GRID HORIZONTAL) ======
            int columns = 8;  // 8 colunas
            int rows = 6;     // 6 linhas
            float buttonWidth = 0.115f;
            float buttonHeight = 0.115f;
            float spacingX = 0.008f;
            float spacingY = 0.012f;
            float startX = 0.02f;
            float startY = 0.84f;
            
            int startIndex = pagina * skinsPerPage;
            int endIndex = Math.Min(startIndex + skinsPerPage, skins.Count);
            
            for (int i = startIndex; i < endIndex; i++)
            {
                int relativeIndex = i - startIndex;
                int row = relativeIndex / columns;
                int col = relativeIndex % columns;
                
                float xMin = startX + (col * (buttonWidth + spacingX));
                float xMax = xMin + buttonWidth;
                float yMax = startY - (row * (buttonHeight + spacingY));
                float yMin = yMax - buttonHeight;
                
                ulong skinId = skins[i];
                bool isFavorito = playerData.ContainsKey(player.userID) && 
                                 playerData[player.userID].Favoritos.Contains(skinId);
                
                // Cores: Azul para skins normais, Dourado para favoritos
                string buttonColor = isFavorito ? "0.9 0.7 0.2 1" : "0.25 0.45 0.75 0.9";
                
                // Painel de fundo do botão
                string skinButtonPanel = container.Add(new CuiPanel
                {
                    Image = { Color = buttonColor },
                    RectTransform = { AnchorMin = $"{xMin} {yMin}", AnchorMax = $"{xMax} {yMax}" }
                }, mainPanel);
                
                // Imagem da skin (item icon)
                // Tentar carregar do ImageLibrary ou usar URL direto
                string imageUrl = GetSkinImageUrl(item.info.shortname, skinId);
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    container.Add(new CuiElement
                    {
                        Parent = skinButtonPanel,
                        Components =
                        {
                            new CuiRawImageComponent 
                            { 
                                Url = imageUrl,
                                Color = "1 1 1 1"
                            },
                            new CuiRectTransformComponent 
                            { 
                                AnchorMin = "0.1 0.2", 
                                AnchorMax = "0.9 0.95" 
                            }
                        }
                    });
                }
                else
                {
                    // Fallback: Mostrar ícone do item sem skin
                    container.Add(new CuiElement
                    {
                        Parent = skinButtonPanel,
                        Components =
                        {
                            new CuiRawImageComponent 
                            { 
                                Url = $"https://rustlabs.com/img/items180/{item.info.shortname}.png",
                                Color = "1 1 1 0.5"
                            },
                            new CuiRectTransformComponent 
                            { 
                                AnchorMin = "0.1 0.2", 
                                AnchorMax = "0.9 0.95" 
                            }
                        }
                    });
                }
                
                // Botão invisível clicável (sobre a imagem)
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.apply {skinId}", Color = "0 0 0 0" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                    Text = { Text = "", FontSize = 1, Align = TextAnchor.MiddleCenter }
                }, skinButtonPanel);
                
                // Label com ID na parte inferior (opcional)
                if (configData.MostrarIDs)
                {
                    container.Add(new CuiLabel
                    {
                        Text = { 
                            Text = $"{skinId}", 
                            FontSize = 8, 
                            Align = TextAnchor.LowerCenter,
                            Color = "0 0 0 0.8"
                        },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.18" }
                    }, skinButtonPanel);
                }
                
                // Estrela de favorito (canto superior direito do botão)
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.fav {skinId}", Color = "0 0 0 0.8" },
                    RectTransform = { AnchorMin = "0.75 0.75", AnchorMax = "1 1" },
                    Text = { 
                        Text = isFavorito ? "★" : "☆", 
                        FontSize = 18, 
                        Align = TextAnchor.MiddleCenter,
                        Color = isFavorito ? "1 0.9 0 1" : "0.8 0.8 0.8 1"
                    }
                }, skinButtonPanel);
            }
            
            // ====== FOOTER (BARRA INFERIOR COM BOTÕES) ======
            string footerPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.12 0.12 0.12 1" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.08" }
            }, mainPanel);
            
            // Botão: Página Anterior
            if (pagina > 0)
            {
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.page {pagina - 1}", Color = "0.3 0.55 0.85 1" },
                    RectTransform = { AnchorMin = "0.02 0.15", AnchorMax = "0.12 0.85" },
                    Text = { Text = "◄ ANTERIOR", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, footerPanel);
            }
            else
            {
                // Botão desabilitado
                container.Add(new CuiButton
                {
                    Button = { Command = "", Color = "0.2 0.2 0.2 0.5" },
                    RectTransform = { AnchorMin = "0.02 0.15", AnchorMax = "0.12 0.85" },
                    Text = { Text = "◄ ANTERIOR", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "0.4 0.4 0.4 1" }
                }, footerPanel);
            }
            
            // Label: Página Atual
            container.Add(new CuiLabel
            {
                Text = { 
                    Text = $"Página {pagina + 1}/{totalPaginas}", 
                    FontSize = 16, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "0.8 0.9 1 1"
                },
                RectTransform = { AnchorMin = "0.14 0.15", AnchorMax = "0.30 0.85" }
            }, footerPanel);
            
            // Botão: Remover Skin
            container.Add(new CuiButton
            {
                Button = { Command = "itemskins.apply 0", Color = "0.85 0.3 0.3 1" },
                RectTransform = { AnchorMin = "0.32 0.15", AnchorMax = "0.46 0.85" },
                Text = { Text = "🗑️ REMOVER", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, footerPanel);
            
            // Botão: Fechar
            container.Add(new CuiButton
            {
                Button = { Command = "itemskins.close", Color = "0.5 0.5 0.5 1" },
                RectTransform = { AnchorMin = "0.54 0.15", AnchorMax = "0.68 0.85" },
                Text = { Text = "❌ FECHAR", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, footerPanel);
            
            // Botão: Próxima Página
            if (pagina < totalPaginas - 1)
            {
                container.Add(new CuiButton
                {
                    Button = { Command = $"itemskins.page {pagina + 1}", Color = "0.3 0.55 0.85 1" },
                    RectTransform = { AnchorMin = "0.88 0.15", AnchorMax = "0.98 0.85" },
                    Text = { Text = "PRÓXIMA ►", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
                }, footerPanel);
            }
            else
            {
                // Botão desabilitado
                container.Add(new CuiButton
                {
                    Button = { Command = "", Color = "0.2 0.2 0.2 0.5" },
                    RectTransform = { AnchorMin = "0.88 0.15", AnchorMax = "0.98 0.85" },
                    Text = { Text = "PRÓXIMA ►", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "0.4 0.4 0.4 1" }
                }, footerPanel);
            }
            
            CuiHelper.AddUi(player, container);
        }
        
        private void DestruirUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "SkinsPanel");
        }
        
        private string GetSkinImageUrl(string itemShortname, ulong skinId)
        {
            // Se ImageLibrary estiver disponível, tentar pegar do cache
            if (ImageLibrary != null && ImageLibrary.IsLoaded)
            {
                string cachedImage = (string)ImageLibrary.Call("GetImage", itemShortname, skinId);
                if (!string.IsNullOrEmpty(cachedImage))
                {
                    return cachedImage;
                }
            }
            
            // Fallback: Usar URL direto da Steam Workshop
            if (skinId > 0)
            {
                // URL do item da Steam Workshop
                return $"https://files.facepunch.com/umod/workshopicons/{skinId}.png";
            }
            
            // Se não tem skin, retornar null para usar o ícone padrão
            return null;
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
            int currentPage = playerCurrentPage.ContainsKey(player.userID) ? playerCurrentPage[player.userID] : 0;
            timer.Once(0.1f, () => {
                if (player != null && player.IsConnected)
                {
                    MostrarUISkinsItem(player, item, currentPage);
                }
            });
        }
        
        [ConsoleCommand("itemskins.page")]
        private void CmdChangePage(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            
            if (arg.Args == null || arg.Args.Length == 0) return;
            
            int pagina;
            if (!int.TryParse(arg.Args[0], out pagina)) return;
            
            var item = player.GetActiveItem();
            if (item == null) return;
            
            MostrarUISkinsItem(player, item, pagina);
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
                int currentPage = playerCurrentPage.ContainsKey(player.userID) ? playerCurrentPage[player.userID] : 0;
                timer.Once(0.1f, () => {
                    if (player != null && player.IsConnected)
                    {
                        MostrarUISkinsItem(player, item, currentPage);
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
        
        [ConsoleCommand("itemskins.reload")]
        private void CmdReloadSkins(ConsoleSystem.Arg arg)
        {
            if (arg.Player() != null && !arg.Player().IsAdmin)
            {
                arg.ReplyWith("Você precisa ser administrador para usar este comando!");
                return;
            }
            
            CarregarSkinsPersonalizadas();
            
            int totalSkins = 0;
            foreach (var skins in itemSkins.Values)
            {
                totalSkins += skins.Count;
            }
            
            arg.ReplyWith($"✅ Skins recarregadas! Total: {totalSkins} skins em {itemSkins.Count} categorias de itens.");
            Puts($"✅ Skins recarregadas por administrador. Total: {totalSkins} skins.");
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
