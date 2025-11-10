# 🔧 Como Corrigir os Plugins Agora

## Método 1: Script Automático (Recomendado) 🤖

Criei um script Python que corrige automaticamente os erros!

### Passo a Passo:

1. **Baixe o script** `corrigir_out_var.py` deste repositório

2. **Execute no servidor via SSH:**
```bash
# Navegue até a pasta dos plugins
cd /caminho/do/servidor/oxide/plugins/

# Execute o script para cada arquivo
python3 /caminho/corrigir_out_var.py BuildTraining.cs
python3 /caminho/corrigir_out_var.py PvPTraining.cs
```

3. **O script vai:**
   - ✅ Criar backup automático (.backup)
   - ✅ Corrigir todos os `out var`
   - ✅ Corrigir todos os `out int`, `out string`, etc
   - ✅ Adicionar `using Oxide.Game.Rust.Cui;` se necessário

4. **Recarregue os plugins:**
```bash
# No console do servidor
oxide.reload BuildTraining
oxide.reload PvPTraining
```

---

## Método 2: Correção Manual 📝

Se não puder usar o script, siga estas instruções:

### Para BuildTraining.cs (linha 553):

1. Abra o arquivo no editor
2. Vá para **linha 553**
3. Procure algo como:
```csharp
if (algo.TryGetValue(key, out var value))
```

4. Mude para:
```csharp
var value;  // ou tipo específico (int, string, etc)
if (algo.TryGetValue(key, out value))
```

### Para PvPTraining.cs:

1. Abra o arquivo no editor
2. No **topo**, após os outros `using`, adicione:
```csharp
using Oxide.Game.Rust.Cui;
```

3. O arquivo deve começar assim:
```csharp
using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json;
using UnityEngine;
using Oxide.Game.Rust.Cui;  // ← ADICIONE ESTA LINHA

namespace Oxide.Plugins
{
    // resto do código...
}
```

---

## Método 3: Me Envie os Arquivos 📤

Para eu corrigir diretamente:

### Opção A - Envie os arquivos completos:
Copie e cole aqui o conteúdo de:
- BuildTraining.cs (completo)
- PvPTraining.cs (completo)

### Opção B - Envie apenas os trechos:

**Para BuildTraining.cs:**
Copie e cole as linhas **545-565** (contexto ao redor do erro)

**Para PvPTraining.cs:**
Copie e cole:
1. Linhas **1-30** (os usings do topo)
2. Linhas **1010-1025** (contexto ao redor do erro)

---

## Exemplos de Correção Comuns

### Exemplo 1: Dictionary.TryGetValue
```csharp
// ❌ ANTES (ERRO)
if (playerData.TryGetValue(player.userID, out var data))
{
    data.Kills++;
}

// ✅ DEPOIS (FUNCIONA)
PlayerData data;
if (playerData.TryGetValue(player.userID, out data))
{
    data.Kills++;
}
```

### Exemplo 2: int.TryParse
```csharp
// ❌ ANTES (ERRO)
if (int.TryParse(args[0], out int amount))
{
    GiveAmount(amount);
}

// ✅ DEPOIS (FUNCIONA)
int amount;
if (int.TryParse(args[0], out amount))
{
    GiveAmount(amount);
}
```

### Exemplo 3: FindPlayer
```csharp
// ❌ ANTES (ERRO)
if (FindPlayer(name, out var target))
{
    target.Heal(100);
}

// ✅ DEPOIS (FUNCIONA)
BasePlayer target;
if (FindPlayer(name, out target))
{
    target.Heal(100);
}
```

### Exemplo 4: Múltiplos out
```csharp
// ❌ ANTES (ERRO)
if (float.TryParse(args[0], out float x) && 
    float.TryParse(args[1], out float y))
{
    DoSomething(x, y);
}

// ✅ DEPOIS (FUNCIONA)
float x, y;
if (float.TryParse(args[0], out x) && 
    float.TryParse(args[1], out y))
{
    DoSomething(x, y);
}
```

---

## Como Encontrar Todos os Problemas

### No Linux/Mac (via SSH):
```bash
# Procurar 'out var'
grep -n "out var" BuildTraining.cs

# Procurar 'out int', 'out string', etc
grep -n "out int\|out string\|out float\|out bool" BuildTraining.cs

# Ver contexto (5 linhas antes e depois)
grep -n -C 5 "out var" BuildTraining.cs
```

### No Windows (PowerShell):
```powershell
# Procurar 'out var'
Select-String -Path "BuildTraining.cs" -Pattern "out var" -Context 2,2

# Ver linha específica
Get-Content BuildTraining.cs | Select-Object -Index (553-1)
```

### Via FTP:
1. Baixe o arquivo para seu PC
2. Abra no Notepad++ ou VS Code
3. Use Ctrl+F para buscar "out var"
4. Use Ctrl+G para ir para linha 553

---

## Verificação Final

Após corrigir, verifique:

```bash
# No console do servidor
oxide.reload BuildTraining
oxide.reload PvPTraining

# Verificar se carregou sem erros
oxide.plugins
```

Se ainda der erro, me envie:
- O novo erro que apareceu
- As linhas ao redor do erro

---

## Precisa de Ajuda Imediata?

### Opção Rápida: Me envie via FTP

1. **Baixe os arquivos** BuildTraining.cs e PvPTraining.cs do servidor
2. **Cole o conteúdo aqui** (pode ser em mensagens separadas se for muito grande)
3. **Eu corrijo** e devolvo os arquivos prontos
4. **Você faz upload** de volta para o servidor

### O que eu preciso:

```
=== BuildTraining.cs ===
[cole o conteúdo completo aqui]

=== PvPTraining.cs ===
[cole o conteúdo completo aqui]
```

---

## 📞 Status Atual

- ❌ BuildTraining.cs → linha 553 → `out var`
- ❌ PvPTraining.cs → linha 1019 → falta `using Oxide.Game.Rust.Cui;`

**Escolha um método acima e vamos resolver!** 🚀
