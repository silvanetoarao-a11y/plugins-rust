# 💻 Exemplos Práticos de Correção de Código

## 🔧 Correções Comuns para C# 6.0 (Oxide)

### 1. TryGetValue com Dictionary

```csharp
// ❌ ERRO - C# 7.0+
private Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

void SomeMethod(BasePlayer player)
{
    if (players.TryGetValue(player.userID, out var data))
    {
        data.Kills++;
    }
}

// ✅ CORRETO - C# 6.0
private Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();

void SomeMethod(BasePlayer player)
{
    PlayerData data;  // Declarar ANTES
    if (players.TryGetValue(player.userID, out data))
    {
        data.Kills++;
    }
}
```

### 2. TryParse com Números

```csharp
// ❌ ERRO - C# 7.0+
[ChatCommand("give")]
void GiveCommand(BasePlayer player, string cmd, string[] args)
{
    if (int.TryParse(args[0], out int amount))
    {
        GiveItem(player, amount);
    }
}

// ✅ CORRETO - C# 6.0
[ChatCommand("give")]
void GiveCommand(BasePlayer player, string cmd, string[] args)
{
    int amount;  // Declarar ANTES
    if (int.TryParse(args[0], out amount))
    {
        GiveItem(player, amount);
    }
}
```

### 3. Múltiplos Out Parameters

```csharp
// ❌ ERRO - C# 7.0+
void ParseCoordinates(string[] args)
{
    if (float.TryParse(args[0], out float x) && 
        float.TryParse(args[1], out float y) && 
        float.TryParse(args[2], out float z))
    {
        TeleportPlayer(new Vector3(x, y, z));
    }
}

// ✅ CORRETO - C# 6.0
void ParseCoordinates(string[] args)
{
    float x, y, z;  // Declarar ANTES
    if (float.TryParse(args[0], out x) && 
        float.TryParse(args[1], out y) && 
        float.TryParse(args[2], out z))
    {
        TeleportPlayer(new Vector3(x, y, z));
    }
}
```

### 4. FindPlayer/BasePlayer.Find

```csharp
// ❌ ERRO - C# 7.0+
[ChatCommand("tp")]
void TeleportCommand(BasePlayer player, string cmd, string[] args)
{
    if (FindPlayer(args[0], out var target))
    {
        player.Teleport(target.transform.position);
    }
}

// ✅ CORRETO - C# 6.0
[ChatCommand("tp")]
void TeleportCommand(BasePlayer player, string cmd, string[] args)
{
    BasePlayer target;  // Declarar ANTES
    if (FindPlayer(args[0], out target))
    {
        player.Teleport(target.transform.position);
    }
}
```

### 5. Enum.TryParse

```csharp
// ❌ ERRO - C# 7.0+
void SetMode(string modeName)
{
    if (Enum.TryParse<GameMode>(modeName, out var mode))
    {
        currentMode = mode;
    }
}

// ✅ CORRETO - C# 6.0
void SetMode(string modeName)
{
    GameMode mode;  // Declarar ANTES
    if (Enum.TryParse<GameMode>(modeName, out mode))
    {
        currentMode = mode;
    }
}
```

---

## 🎨 Correções para CuiHelper (Custom UI)

### Template Completo com UI

```csharp
using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json;
using UnityEngine;
using Oxide.Game.Rust.Cui;  // ← ESSENCIAL PARA CuiHelper

namespace Oxide.Plugins
{
    [Info("ExampleUI", "YourName", "1.0.0")]
    class ExampleUI : RustPlugin
    {
        // Criar UI simples
        void ShowUI(BasePlayer player)
        {
            var elements = new CuiElementContainer();
            
            // Painel principal
            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.3 0.3", AnchorMax = "0.7 0.7" },
                CursorEnabled = true
            }, "Overlay", "MyUIPanel");
            
            // Texto
            elements.Add(new CuiLabel
            {
                Text = { 
                    Text = "Olá, mundo!", 
                    FontSize = 20, 
                    Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                },
                RectTransform = { AnchorMin = "0 0.5", AnchorMax = "1 1" }
            }, "MyUIPanel");
            
            // Botão
            elements.Add(new CuiButton
            {
                Button = { 
                    Command = "myui.close", 
                    Color = "0.8 0.2 0.2 1" 
                },
                RectTransform = { AnchorMin = "0.3 0.2", AnchorMax = "0.7 0.4" },
                Text = { 
                    Text = "Fechar", 
                    FontSize = 16, 
                    Align = TextAnchor.MiddleCenter 
                }
            }, "MyUIPanel");
            
            // Adicionar ao jogador
            CuiHelper.AddUi(player, elements);
        }
        
        // Fechar UI
        void CloseUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "MyUIPanel");
        }
        
        [ConsoleCommand("myui.close")]
        void CloseUICommand(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null)
            {
                CloseUI(player);
            }
        }
    }
}
```

### Exemplo de UI com Imagem

```csharp
void ShowImageUI(BasePlayer player)
{
    var elements = new CuiElementContainer();
    
    // Painel de fundo
    elements.Add(new CuiPanel
    {
        Image = { Color = "0 0 0 0.95" },
        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
        CursorEnabled = true
    }, "Overlay", "ImagePanel");
    
    // Imagem (URL ou asset)
    elements.Add(new CuiElement
    {
        Parent = "ImagePanel",
        Components =
        {
            new CuiRawImageComponent 
            { 
                Url = "https://exemplo.com/imagem.png",
                Sprite = "assets/content/textures/generic/fulltransparent.tga"
            },
            new CuiRectTransformComponent 
            { 
                AnchorMin = "0.4 0.4", 
                AnchorMax = "0.6 0.6" 
            }
        }
    });
    
    CuiHelper.AddUi(player, elements);
}
```

### Exemplo de UI com Input

```csharp
void ShowInputUI(BasePlayer player)
{
    var elements = new CuiElementContainer();
    
    // Painel
    elements.Add(new CuiPanel
    {
        Image = { Color = "0.1 0.1 0.1 0.9" },
        RectTransform = { AnchorMin = "0.35 0.4", AnchorMax = "0.65 0.6" }
    }, "Overlay", "InputPanel");
    
    // Label
    elements.Add(new CuiLabel
    {
        Text = { 
            Text = "Digite um valor:", 
            FontSize = 14, 
            Align = TextAnchor.MiddleCenter 
        },
        RectTransform = { AnchorMin = "0.1 0.6", AnchorMax = "0.9 0.9" }
    }, "InputPanel");
    
    // Input field
    elements.Add(new CuiElement
    {
        Parent = "InputPanel",
        Components =
        {
            new CuiInputFieldComponent
            {
                FontSize = 14,
                Command = "myui.submit",
                Text = ""
            },
            new CuiRectTransformComponent 
            { 
                AnchorMin = "0.1 0.35", 
                AnchorMax = "0.9 0.55" 
            }
        }
    });
    
    // Botão enviar
    elements.Add(new CuiButton
    {
        Button = { Command = "myui.submit", Color = "0.3 0.8 0.3 1" },
        RectTransform = { AnchorMin = "0.3 0.1", AnchorMax = "0.7 0.3" },
        Text = { Text = "Enviar", FontSize = 12, Align = TextAnchor.MiddleCenter }
    }, "InputPanel");
    
    CuiHelper.AddUi(player, elements);
}

[ConsoleCommand("myui.submit")]
void SubmitCommand(ConsoleSystem.Arg arg)
{
    var player = arg.Player();
    if (player == null) return;
    
    string input = arg.GetString(0);
    Puts($"Jogador {player.displayName} enviou: {input}");
    
    CuiHelper.DestroyUi(player, "InputPanel");
}
```

---

## 🔄 Padrões de Migração Comuns

### Pattern Matching (NÃO funciona no Oxide)

```csharp
// ❌ ERRO - C# 7.0+
void ProcessEntity(BaseEntity entity)
{
    if (entity is BasePlayer player)
    {
        player.Heal(100);
    }
}

// ✅ CORRETO - C# 6.0
void ProcessEntity(BaseEntity entity)
{
    var player = entity as BasePlayer;
    if (player != null)
    {
        player.Heal(100);
    }
}
```

### Tuples (NÃO funciona no Oxide)

```csharp
// ❌ ERRO - C# 7.0+
(int x, int y) GetCoordinates()
{
    return (10, 20);
}

// ✅ CORRETO - C# 6.0
class Coordinates
{
    public int X { get; set; }
    public int Y { get; set; }
}

Coordinates GetCoordinates()
{
    return new Coordinates { X = 10, Y = 20 };
}
```

### Local Functions (NÃO funciona no Oxide)

```csharp
// ❌ ERRO - C# 7.0+
void MainMethod()
{
    void HelperFunction()
    {
        // código
    }
    
    HelperFunction();
}

// ✅ CORRETO - C# 6.0
void MainMethod()
{
    HelperFunction();
}

void HelperFunction()
{
    // código
}
```

---

## 🎯 Checklist de Compatibilidade C# 6.0

✅ **Pode usar:**
- Lambda expressions: `() => { }`
- LINQ: `.Where()`, `.Select()`, etc
- Async/await (com limitações)
- String interpolation: `$"Hello {name}"`
- Null-conditional: `player?.displayName`
- Null-coalescing: `value ?? defaultValue`
- Auto-properties: `public int Value { get; set; }`
- Expression bodied members: `int GetValue() => 42;`

❌ **NÃO pode usar:**
- Out variables: `out var x`
- Pattern matching: `is Type variable`
- Tuples: `(int, string)`
- Local functions
- Throw expressions: `value ?? throw new Exception()`
- Binary literals: `0b1010`
- Digit separators: `1_000_000`

---

## 📝 Script de Verificação Rápida

Use este script para encontrar problemas:

```bash
#!/bin/bash
# verificar_plugin.sh

echo "Verificando problemas comuns..."

# Verificar out var
echo "=== Procurando 'out var' (C# 7.0+) ==="
grep -n "out var" "$1"

# Verificar pattern matching
echo "=== Procurando 'is.*var' (pattern matching) ==="
grep -n "is [A-Z].*\svar\s" "$1"

# Verificar CuiHelper sem using
echo "=== Verificando CuiHelper ==="
if grep -q "CuiHelper" "$1"; then
    if ! grep -q "using Oxide.Game.Rust.Cui" "$1"; then
        echo "AVISO: CuiHelper usado mas falta 'using Oxide.Game.Rust.Cui;'"
    fi
fi

echo "Verificação concluída!"
```

**Uso:**
```bash
chmod +x verificar_plugin.sh
./verificar_plugin.sh BuildTraining.cs
```

---

**Precisa de mais ajuda? Cole o código específico que está com erro!** 🚀
