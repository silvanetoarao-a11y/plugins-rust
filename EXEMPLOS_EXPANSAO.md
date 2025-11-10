# Exemplos de Expansão do Plugin

Este documento contém exemplos de como expandir o WelcomePlugin com novas funcionalidades.

## 📍 Sistema de Teleporte

### Adicionar comando de home

```csharp
private Dictionary<ulong, Vector3> playerHomes = new Dictionary<ulong, Vector3>();

[ChatCommand("sethome")]
private void SetHomeCommand(BasePlayer player, string command, string[] args)
{
    playerHomes[player.userID] = player.transform.position;
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Home definida com sucesso!");
}

[ChatCommand("home")]
private void HomeCommand(BasePlayer player, string command, string[] args)
{
    if (!playerHomes.ContainsKey(player.userID))
    {
        SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Você não definiu uma home! Use /sethome</color>");
        return;
    }
    
    timer.Once(5f, () =>
    {
        if (player != null && player.IsConnected)
        {
            player.Teleport(playerHomes[player.userID]);
            SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Teleportado para sua home!");
        }
    });
    
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Teleportando em 5 segundos...");
}
```

## 💰 Sistema de Economia Simples

### Adicionar sistema de moedas

```csharp
private Dictionary<ulong, int> playerCoins = new Dictionary<ulong, int>();

private void GiveCoins(BasePlayer player, int amount)
{
    if (!playerCoins.ContainsKey(player.userID))
    {
        playerCoins[player.userID] = 0;
    }
    playerCoins[player.userID] += amount;
}

[ChatCommand("coins")]
private void CoinsCommand(BasePlayer player, string command, string[] args)
{
    int coins = playerCoins.ContainsKey(player.userID) ? playerCoins[player.userID] : 0;
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você tem {coins} moedas!");
}

[ChatCommand("daily")]
private void DailyCommand(BasePlayer player, string command, string[] args)
{
    // Verifica se já pegou hoje (você precisaria adicionar controle de tempo)
    GiveCoins(player, 100);
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você recebeu 100 moedas diárias!");
}

// Dar moedas quando matar um jogador
void OnPlayerDeath(BasePlayer victim, HitInfo info)
{
    var attacker = info?.InitiatorPlayer;
    if (attacker != null && victim != null && attacker != victim)
    {
        GiveCoins(attacker, 50);
        SendReply(attacker, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> +50 moedas por eliminar {victim.displayName}!");
    }
}
```

## 🛡️ Sistema de Proteção Anti-Raid

### Proteger jogadores iniciantes

```csharp
private HashSet<ulong> protectedPlayers = new HashSet<ulong>();
private Dictionary<ulong, DateTime> playerJoinTimes = new Dictionary<ulong, DateTime>();

void OnPlayerConnected(BasePlayer player)
{
    if (!playerJoinTimes.ContainsKey(player.userID))
    {
        playerJoinTimes[player.userID] = DateTime.Now;
        protectedPlayers.Add(player.userID);
        SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você está protegido por 24 horas!");
    }
}

object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
{
    var player = entity as BasePlayer;
    if (player == null) return null;
    
    // Se o jogador está protegido
    if (protectedPlayers.Contains(player.userID))
    {
        if (DateTime.Now - playerJoinTimes[player.userID] < TimeSpan.FromHours(24))
        {
            var attacker = info?.InitiatorPlayer;
            if (attacker != null)
            {
                SendReply(attacker, $"<color=#FF0000>{config.ChatPrefix} Este jogador está protegido!</color>");
                return true; // Bloqueia o dano
            }
        }
        else
        {
            protectedPlayers.Remove(player.userID);
        }
    }
    
    return null;
}
```

## 🎁 Sistema de Recompensas por Tempo Online

### Recompensar jogadores por jogar

```csharp
private Dictionary<ulong, DateTime> sessionStart = new Dictionary<ulong, DateTime>();

void OnPlayerConnected(BasePlayer player)
{
    sessionStart[player.userID] = DateTime.Now;
}

[ChatCommand("recompensa")]
private void RewardCommand(BasePlayer player, string command, string[] args)
{
    if (!sessionStart.ContainsKey(player.userID))
    {
        SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Erro ao calcular tempo!</color>");
        return;
    }
    
    TimeSpan timeOnline = DateTime.Now - sessionStart[player.userID];
    int hours = (int)timeOnline.TotalHours;
    
    if (hours < 1)
    {
        SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Você precisa jogar por pelo menos 1 hora!</color>");
        return;
    }
    
    // Dar recompensa baseada nas horas
    int scrapReward = hours * 50;
    Item scrap = ItemManager.CreateByName("scrap", scrapReward);
    player.GiveItem(scrap);
    
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Você jogou por {hours} hora(s) e recebeu {scrapReward} scrap!");
    
    // Reseta o contador
    sessionStart[player.userID] = DateTime.Now;
}
```

## 🎯 Sistema de Missões Diárias

### Adicionar missões simples

```csharp
private Dictionary<ulong, Dictionary<string, int>> dailyQuests = new Dictionary<ulong, Dictionary<string, int>>();

void InitializeDailyQuests(BasePlayer player)
{
    if (!dailyQuests.ContainsKey(player.userID))
    {
        dailyQuests[player.userID] = new Dictionary<string, int>
        {
            { "kills", 0 },      // Matar 5 jogadores
            { "wood", 0 },       // Coletar 1000 madeira
            { "stone", 0 }       // Coletar 1000 pedra
        };
    }
}

[ChatCommand("missoes")]
private void QuestsCommand(BasePlayer player, string command, string[] args)
{
    InitializeDailyQuests(player);
    var quests = dailyQuests[player.userID];
    
    SendReply(player, $"<color={config.PrefixColor}>===  MISSÕES DIÁRIAS ===</color>");
    SendReply(player, $"Eliminar jogadores: {quests["kills"]}/5");
    SendReply(player, $"Coletar madeira: {quests["wood"]}/1000");
    SendReply(player, $"Coletar pedra: {quests["stone"]}/1000");
    
    // Verificar e dar recompensas
    if (quests["kills"] >= 5 && quests["wood"] >= 1000 && quests["stone"] >= 1000)
    {
        SendReply(player, $"<color=#FFD700>🎉 Todas as missões completas! Use /recompensas para coletar!</color>");
    }
}

void OnDispenserGather(ResourceDispenser dispenser, BasePlayer player, Item item)
{
    InitializeDailyQuests(player);
    
    if (item.info.shortname == "wood")
    {
        dailyQuests[player.userID]["wood"] += item.amount;
    }
    else if (item.info.shortname == "stones")
    {
        dailyQuests[player.userID]["stone"] += item.amount;
    }
}
```

## 🏆 Sistema de Ranking

### Adicionar placar de jogadores

```csharp
private Dictionary<ulong, int> playerKills = new Dictionary<ulong, int>();

void OnPlayerDeath(BasePlayer victim, HitInfo info)
{
    var attacker = info?.InitiatorPlayer;
    if (attacker != null && victim != null && attacker != victim)
    {
        if (!playerKills.ContainsKey(attacker.userID))
        {
            playerKills[attacker.userID] = 0;
        }
        playerKills[attacker.userID]++;
    }
}

[ChatCommand("top")]
private void TopCommand(BasePlayer player, string command, string[] args)
{
    var sortedKills = playerKills.OrderByDescending(x => x.Value).Take(10);
    
    SendReply(player, $"<color={config.PrefixColor}>===  TOP 10 JOGADORES ===</color>");
    
    int position = 1;
    foreach (var entry in sortedKills)
    {
        var topPlayer = BasePlayer.FindByID(entry.Key);
        string name = topPlayer != null ? topPlayer.displayName : "Desconhecido";
        SendReply(player, $"{position}. {name} - {entry.Value} kills");
        position++;
    }
}
```

## 📢 Sistema de Anúncios Automáticos

### Enviar mensagens periódicas

```csharp
void OnServerInitialized()
{
    // Anúncios a cada 10 minutos
    timer.Every(600f, () =>
    {
        string[] announcements = new string[]
        {
            "Não esqueça de usar /regras para ver as regras do servidor!",
            "Use /ajuda para ver todos os comandos disponíveis!",
            "Precisando de ajuda? Entre no nosso Discord!",
            "Jogue limpo e divirta-se!"
        };
        
        int index = UnityEngine.Random.Range(0, announcements.Length);
        Server.Broadcast($"<color={config.PrefixColor}>{config.ChatPrefix}</color> {announcements[index]}");
    });
}
```

## 🎮 Sistema de Kits Múltiplos

### Adicionar vários kits diferentes

```csharp
private Dictionary<string, Dictionary<string, int>> kits = new Dictionary<string, Dictionary<string, int>>()
{
    ["pvp"] = new Dictionary<string, int>
    {
        { "rifle.ak", 1 },
        { "ammo.rifle", 120 },
        { "syringe.medical", 5 },
        { "metal.facemask", 1 },
        { "metal.plate.torso", 1 }
    },
    ["builder"] = new Dictionary<string, int>
    {
        { "wood", 5000 },
        { "stone", 5000 },
        { "metal.fragments", 2000 },
        { "hammer", 1 },
        { "building.planner", 1 }
    },
    ["farmer"] = new Dictionary<string, int>
    {
        { "seed.corn", 20 },
        { "seed.hemp", 20 },
        { "seed.pumpkin", 20 },
        { "water.bucket", 1 },
        { "hoe", 1 }
    }
};

[ChatCommand("kit")]
private void KitCommand(BasePlayer player, string command, string[] args)
{
    if (args.Length == 0)
    {
        SendReply(player, $"<color={config.PrefixColor}>Kits disponíveis:</color>");
        SendReply(player, "/kit pvp - Kit para combate");
        SendReply(player, "/kit builder - Kit para construção");
        SendReply(player, "/kit farmer - Kit para agricultura");
        return;
    }
    
    string kitName = args[0].ToLower();
    
    if (!kits.ContainsKey(kitName))
    {
        SendReply(player, $"<color=#FF0000>{config.ChatPrefix} Kit não encontrado!</color>");
        return;
    }
    
    // Verificar cooldown (adicione o sistema de cooldown)
    GiveKit(player, kits[kitName]);
    SendReply(player, $"<color={config.PrefixColor}>{config.ChatPrefix}</color> Kit '{kitName}' recebido!");
}

private void GiveKit(BasePlayer player, Dictionary<string, int> items)
{
    foreach (var item in items)
    {
        Item itemToGive = ItemManager.CreateByName(item.Key, item.Value);
        if (itemToGive != null)
        {
            player.GiveItem(itemToGive);
        }
    }
}
```

## 💾 Sistema de Dados Persistentes

### Salvar dados entre reinicializações

```csharp
private StoredData storedData;

class StoredData
{
    public Dictionary<ulong, PlayerData> Players = new Dictionary<ulong, PlayerData>();
}

class PlayerData
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Coins { get; set; }
    public DateTime LastLogin { get; set; }
}

void LoadData()
{
    try
    {
        storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("WelcomePluginData");
    }
    catch
    {
        storedData = new StoredData();
    }
}

void SaveData()
{
    Interface.Oxide.DataFileSystem.WriteObject("WelcomePluginData", storedData);
}

void Init()
{
    LoadData();
}

void Unload()
{
    SaveData();
}

void OnServerSave()
{
    SaveData();
}
```

---

## 🔧 Dicas de Implementação

1. **Sempre teste cada funcionalidade** antes de adicionar ao servidor de produção
2. **Use permissões** para controlar quem pode usar comandos específicos
3. **Adicione cooldowns** para evitar spam de comandos
4. **Salve dados importantes** para não perder progresso dos jogadores
5. **Otimize timers** para não sobrecarregar o servidor
6. **Trate erros** adequadamente para evitar crashes

## 📚 Recursos Úteis

- [uMod API Documentation](https://umod.org/documentation/api)
- [Rust Item List](https://rustlabs.com/)
- [Oxide Forums](https://umod.org/community/)
- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

**Boas modificações!** 🚀
