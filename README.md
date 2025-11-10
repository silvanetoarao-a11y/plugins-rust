# Plugin para Servidor Rust

Plugin completo e funcional para servidores de Rust usando uMod/Oxide ou Carbon.

## 📋 Sobre o Plugin

O **WelcomePlugin** é um plugin versátil que adiciona várias funcionalidades úteis ao seu servidor de Rust:

- ✅ Sistema de boas-vindas personalizável
- ✅ Comandos úteis para jogadores
- ✅ Kit inicial para novos jogadores
- ✅ Comandos administrativos
- ✅ Sistema de configuração completo
- ✅ Notificações de entrada/saída de jogadores

## 🚀 Instalação

### Requisitos
- Servidor Rust
- uMod (Oxide) ou Carbon instalado

### Passo a Passo

1. **Baixe o plugin**: Copie o arquivo `WelcomePlugin.cs`

2. **Instale no servidor**: 
   - Coloque o arquivo na pasta `oxide/plugins/` ou `carbon/plugins/`
   - Caminho completo: `<pasta_do_servidor>/oxide/plugins/WelcomePlugin.cs`

3. **Carregue o plugin**:
   - O plugin será compilado e carregado automaticamente
   - Ou use o comando console: `oxide.reload WelcomePlugin`

4. **Configure o plugin**:
   - Após o primeiro carregamento, um arquivo de configuração será criado em:
   - `oxide/config/WelcomePlugin.json`

## ⚙️ Configuração

O arquivo de configuração (`WelcomePlugin.json`) contém as seguintes opções:

```json
{
  "Mensagem de boas-vindas": "Bem-vindo ao servidor, {player}!",
  "Mostrar mensagem no chat": true,
  "Mostrar popup na tela": true,
  "Tempo do popup (segundos)": 5.0,
  "Prefix do chat": "[Servidor]",
  "Cor do prefix (hex)": "#00FF00"
}
```

### Personalizando

- `{player}` - Será substituído pelo nome do jogador
- Cores em formato hexadecimal (ex: `#00FF00` para verde, `#FF0000` para vermelho)
- Ajuste os tempos e mensagens conforme sua preferência

## 📝 Comandos

### Comandos para Jogadores

| Comando | Descrição | Exemplo |
|---------|-----------|---------|
| `/ajuda` | Mostra todos os comandos disponíveis | `/ajuda` |
| `/online` | Mostra quantos jogadores estão online | `/online` |
| `/online lista` | Lista todos os jogadores online | `/online lista` |
| `/regras` | Mostra as regras do servidor | `/regras` |
| `/kit` | Recebe um kit inicial (apenas uma vez) | `/kit` |
| `/pos` | Mostra sua posição atual no mapa | `/pos` |

### Comandos para Administradores

| Comando | Descrição | Exemplo |
|---------|-----------|---------|
| `/heal` | Cura você completamente | `/heal` |
| `/heal <jogador>` | Cura um jogador específico | `/heal Steve` |

### Comandos do Console

| Comando | Descrição |
|---------|-----------|
| `welcomeplugin.reload` | Recarrega a configuração do plugin |

## 🎁 Kit Inicial

O kit inicial inclui:

- 🪵 1000 Wood
- 🪨 1000 Stone
- ⚙️ 500 Metal Fragments
- 🧵 100 Cloth
- ⛏️ 1x Stone Pickaxe
- 🪓 1x Hatchet
- 🏹 1x Hunting Bow
- ➡️ 50x Wooden Arrows
- 🩹 5x Bandages

**Nota**: Cada jogador pode receber o kit apenas uma vez.

## 🎯 Funcionalidades

### Eventos Automáticos

1. **Jogador Conecta**:
   - Mensagem de boas-vindas no chat (se ativado)
   - Popup na tela do jogador (se ativado)
   - Anuncia no chat que o jogador entrou

2. **Jogador Desconecta**:
   - Anuncia no chat que o jogador saiu

3. **Jogador Renasce**:
   - Mensagem de encorajamento

### Permissões

O plugin gerencia automaticamente as seguintes permissões:

- `welcomeplugin.kit.received` - Marca que o jogador já recebeu o kit inicial

## 🔧 Personalização

### Modificar o Kit Inicial

Edite a função `GiveStarterKit` no código:

```csharp
Dictionary<string, int> kitItems = new Dictionary<string, int>
{
    { "wood", 1000 },           // Nome do item, quantidade
    { "stone", 1000 },
    { "metal.fragments", 500 },
    // Adicione mais itens aqui
};
```

### Adicionar Mais Comandos

Adicione novos comandos seguindo este padrão:

```csharp
[ChatCommand("meucomando")]
private void MeuComando(BasePlayer player, string command, string[] args)
{
    SendReply(player, "Mensagem de resposta!");
}
```

### Modificar Regras

Edite a função `RulesCommand` para personalizar as regras do servidor.

## 🐛 Solução de Problemas

### Plugin não carrega

1. Verifique se o arquivo está na pasta correta
2. Verifique o console do servidor para erros de compilação
3. Certifique-se de que o uMod/Oxide está atualizado

### Configuração não funciona

1. Delete o arquivo `WelcomePlugin.json` da pasta `oxide/config/`
2. Recarregue o plugin: `oxide.reload WelcomePlugin`
3. Um novo arquivo de configuração será criado

### Kit não funciona

- Verifique se os nomes dos itens estão corretos
- Consulte a lista de itens do Rust: [rustlabs.com](https://rustlabs.com/)

## 📜 Changelog

### Versão 1.0.0
- ✨ Lançamento inicial
- ✅ Sistema de boas-vindas
- ✅ Comandos básicos
- ✅ Kit inicial
- ✅ Comandos administrativos
- ✅ Sistema de configuração

## 📄 Licença

Este plugin é de código aberto e pode ser modificado livremente para uso em seu servidor.

## 🤝 Contribuições

Sinta-se livre para modificar e melhorar este plugin! Sugestões de melhorias:

- Sistema de teleporte
- Sistema de economia
- Proteção de áreas
- Sistema de clãs
- Eventos automáticos
- Sistema de votação
- Loja de itens

## 📞 Suporte

Para suporte adicional:
- Consulte a documentação do uMod: [umod.org](https://umod.org/)
- Fórum da comunidade Rust
- Discord do servidor

---

**Desenvolvido para a comunidade Rust** 🎮
