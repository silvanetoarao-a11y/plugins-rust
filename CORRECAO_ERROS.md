# 🔧 Correção de Erros - Plugins Rust

## Erro 1: BuildTraining.cs - C# 6.0 Declaration Expression

### ❌ Erro
```
Error while compiling: BuildTraining.cs(553,48): error CS1644: 
Feature `declaration expression' cannot be used because it is not part of the C# 6.0 language specification
```

### 📝 Explicação
O Oxide/uMod compila plugins usando **C# 6.0**, mas o código está usando uma feature do **C# 7.0+** chamada "declaration expression" (declaração inline de variáveis).

### ✅ Solução

**ANTES (C# 7.0 - NÃO FUNCIONA):**
```csharp
// Linha 553 aproximadamente
if (dict.TryGetValue(key, out var value))
{
    // usa value
}

// Ou similar a:
if (TryGetPlayer(args[0], out var player))
{
    // usa player
}
```

**DEPOIS (C# 6.0 - FUNCIONA):**
```csharp
// Declare a variável ANTES
var value = default(TipoDoValor); // ou tipo específico
if (dict.TryGetValue(key, out value))
{
    // usa value
}

// Ou:
BasePlayer player;
if (TryGetPlayer(args[0], out player))
{
    // usa player
}
```

### 🔍 Como Corrigir

1. Abra o arquivo `BuildTraining.cs`
2. Vá para a **linha 553** (ou próxima)
3. Procure por padrões como `out var` ou `out string` ou `out int`
4. Declare a variável ANTES do `if` ou método
5. Remova o `var` ou tipo do `out`

### 📌 Exemplos Comuns

```csharp
// ❌ ERRADO (C# 7.0+)
if (int.TryParse(args[0], out int result))

// ✅ CORRETO (C# 6.0)
int result;
if (int.TryParse(args[0], out result))

// ❌ ERRADO
if (playerData.TryGetValue(player.userID, out var data))

// ✅ CORRETO
PlayerData data;
if (playerData.TryGetValue(player.userID, out data))

// ❌ ERRADO
if (BasePlayer.Find(name, out var target))

// ✅ CORRETO
BasePlayer target;
if (BasePlayer.Find(name, out target))
```

---

## Erro 2: PvPTraining.cs - CuiHelper não existe

### ❌ Erro
```
Error while compiling: PvPTraining.cs(1019,13): error CS0103: 
The name `CuiHelper' does not exist in the current context
```

### 📝 Explicação
O código está tentando usar `CuiHelper` (Cui = Custom UI) mas **falta a referência/using** necessária.

### ✅ Solução Rápida

1. Abra o arquivo `PvPTraining.cs`
2. No **topo do arquivo**, adicione esta linha com os outros `using`:

```csharp
using Oxide.Game.Rust.Cui;
```

### 📄 Exemplo Completo do Topo do Arquivo

**ANTES:**
```csharp
using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("PvPTraining", "Author", "1.0.0")]
    class PvPTraining : RustPlugin
    {
        // código...
    }
}
```

**DEPOIS:**
```csharp
using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;  // ← ADICIONE ESTA LINHA

namespace Oxide.Plugins
{
    [Info("PvPTraining", "Author", "1.0.0")]
    class PvPTraining : RustPlugin
    {
        // código...
    }
}
```

### 🔍 Verificação Adicional

Se após adicionar o `using` ainda der erro, verifique se:

1. **O Oxide está atualizado** (versão mínima 2.0.5000+)
2. **O Carbon está atualizado** (se estiver usando Carbon)
3. Tente recarregar o plugin: `oxide.reload PvPTraining`

### 📌 Outros Namespaces Úteis

Se estiver usando outras features, pode precisar destes:

```csharp
using UnityEngine;                  // Para Vector3, GameObject, etc
using Oxide.Core.Libraries.Covalence; // Para sistema universal
using System.Linq;                   // Para LINQ queries
using System.Text;                   // Para StringBuilder
using Rust;                         // Para features específicas do Rust
using Oxide.Game.Rust.Cui;          // Para Custom UI
```

---

## 🎯 Passo a Passo para Aplicar as Correções

### Para BuildTraining.cs:

1. Faça backup do arquivo original
2. Abra `BuildTraining.cs` no editor
3. Vá para linha 553 (ou busque por "out var")
4. Aplique a correção conforme exemplos acima
5. Salve o arquivo
6. Recarregue: `oxide.reload BuildTraining`
7. Verifique erros no console

### Para PvPTraining.cs:

1. Faça backup do arquivo original
2. Abra `PvPTraining.cs` no editor
3. Adicione `using Oxide.Game.Rust.Cui;` no topo
4. Salve o arquivo
5. Recarregue: `oxide.reload PvPTraining`
6. Verifique erros no console

---

## 🔍 Como Encontrar Outras Ocorrências

### Buscar declaration expressions:

```bash
# No Linux/Mac (via SSH)
grep -n "out var" BuildTraining.cs
grep -n "out string" BuildTraining.cs
grep -n "out int" BuildTraining.cs
grep -n "out float" BuildTraining.cs
```

### No Windows (PowerShell):
```powershell
Select-String -Path "BuildTraining.cs" -Pattern "out var"
```

---

## 📝 Template de Correção Rápida

Se você me enviar o trecho do código com erro (linha 553 do BuildTraining.cs), posso corrigir especificamente para você!

### Como obter o código:

1. Abra o arquivo no editor
2. Copie as linhas 545-560 (contexto ao redor da linha 553)
3. Cole aqui para eu corrigir

---

## 🐛 Outros Erros Comuns do Oxide

### Erro: "Type or namespace could not be found"
**Solução:** Adicione o `using` apropriado

### Erro: "Does not contain a definition for..."
**Solução:** Verifique se a API do Oxide mudou, consulte docs

### Erro: "Unexpected symbol"
**Solução:** Verifique sintaxe, falta de `;` ou `}`

### Erro: "Cannot convert type"
**Solução:** Faça cast explícito: `(TipoDesejado)valor`

---

## 📚 Recursos Úteis

- [Oxide API Documentation](https://umod.org/documentation/api)
- [C# 6.0 Reference](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-6)
- [Oxide UI Framework](https://umod.org/documentation/games/rust/cui)
- [Rust Plugin Examples](https://github.com/OxideMod/Oxide.Rust)

---

## ✅ Checklist de Correção

- [ ] Fiz backup dos arquivos originais
- [ ] Corrigi declaration expressions (out var → declaração antes)
- [ ] Adicionei `using Oxide.Game.Rust.Cui;` no PvPTraining
- [ ] Salvei os arquivos
- [ ] Recarreguei os plugins
- [ ] Testei no servidor
- [ ] Verifiquei console para novos erros

---

**Precisa de ajuda específica? Cole o código com erro que eu corrijo!** 🔧
