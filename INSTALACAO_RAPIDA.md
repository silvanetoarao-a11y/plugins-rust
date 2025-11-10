# 🚀 Instalação Rápida - WelcomePlugin

## Guia de 5 Minutos

### 1️⃣ Pré-requisitos

- ✅ Servidor Rust rodando
- ✅ uMod/Oxide ou Carbon instalado
- ✅ Acesso FTP ou painel de controle do servidor

### 2️⃣ Instalação

#### Opção A: Via FTP

1. Conecte-se ao seu servidor via FTP
2. Navegue até a pasta do servidor
3. Vá para `oxide/plugins/` (ou `carbon/plugins/`)
4. Faça upload do arquivo `WelcomePlugin.cs`
5. O plugin será compilado automaticamente

#### Opção B: Via Painel de Controle

1. Acesse o painel de controle do seu servidor
2. Vá para a seção de plugins/arquivos
3. Navegue até `oxide/plugins/`
4. Faça upload do `WelcomePlugin.cs`

#### Opção C: Via Console do Servidor

```bash
# Se você tem acesso SSH ao servidor
cd /caminho/do/servidor/oxide/plugins/
wget https://seusite.com/WelcomePlugin.cs
# ou copie manualmente o arquivo
```

### 3️⃣ Verificação

1. **Abra o console do servidor**
2. Digite: `oxide.plugins`
3. Você deve ver `WelcomePlugin v1.0.0` na lista

### 4️⃣ Configuração Inicial

1. **Localize o arquivo de config**:
   - Caminho: `oxide/config/WelcomePlugin.json`

2. **Edite conforme necessário**:
```json
{
  "Mensagem de boas-vindas": "Bem-vindo ao MEU SERVIDOR, {player}!",
  "Mostrar mensagem no chat": true,
  "Mostrar popup na tela": true,
  "Tempo do popup (segundos)": 5.0,
  "Prefix do chat": "[MEU SERVIDOR]",
  "Cor do prefix (hex)": "#00FF00"
}
```

3. **Recarregue o plugin**:
```
oxide.reload WelcomePlugin
```

### 5️⃣ Teste

1. **Entre no servidor**
2. Digite `/ajuda` no chat
3. Você deve ver a lista de comandos

### 🎉 Pronto!

Seu plugin está instalado e funcionando!

---

## 🔧 Comandos Úteis do Console

| Comando | Função |
|---------|--------|
| `oxide.plugins` | Lista todos os plugins |
| `oxide.reload WelcomePlugin` | Recarrega o plugin |
| `oxide.unload WelcomePlugin` | Descarrega o plugin |
| `oxide.load WelcomePlugin` | Carrega o plugin |

---

## 🐛 Problemas Comuns

### Plugin não aparece na lista

**Causa**: Erro de compilação

**Solução**:
1. Verifique o console para erros
2. Certifique-se que o Oxide está atualizado
3. Verifique se o arquivo `.cs` não está corrompido

### Comandos não funcionam

**Causa**: Plugin não está carregado

**Solução**:
```
oxide.load WelcomePlugin
```

### Configuração não salva

**Causa**: Permissões de arquivo

**Solução**:
```bash
# No Linux (via SSH)
chmod 644 oxide/config/WelcomePlugin.json
```

### Kit não funciona

**Causa**: Nomes de itens incorretos

**Solução**: 
- Verifique os nomes em [RustLabs](https://rustlabs.com)
- Nomes devem estar em minúsculas
- Use pontos para separar (ex: `metal.fragments`)

---

## 📊 Estrutura de Arquivos

```
servidor-rust/
├── RustDedicated (ou RustDedicated.exe)
└── oxide/
    ├── plugins/
    │   └── WelcomePlugin.cs          ← Coloque aqui
    ├── config/
    │   └── WelcomePlugin.json        ← Gerado automaticamente
    ├── data/
    │   └── WelcomePluginData.json    ← Se usar dados persistentes
    └── logs/
        └── oxide/
            └── debug.log             ← Verifique erros aqui
```

---

## 🔐 Permissões

### Para dar admin temporário:

```
oxide.grant user <nome> admin
```

### Para remover admin:

```
oxide.revoke user <nome> admin
```

---

## 🎯 Próximos Passos

1. ✅ **Personalize as mensagens** no arquivo de configuração
2. ✅ **Ajuste o kit inicial** editando o código
3. ✅ **Adicione suas regras** no comando `/regras`
4. ✅ **Teste todos os comandos** com seus jogadores
5. ✅ **Explore expansões** em `EXEMPLOS_EXPANSAO.md`

---

## 💡 Dicas

- 🔄 Sempre faça backup antes de editar
- 📝 Mantenha um log das suas modificações
- 🧪 Teste em servidor de desenvolvimento primeiro
- 👥 Peça feedback dos jogadores
- 📚 Consulte a documentação do Oxide

---

## 📞 Recursos

- 📖 [Documentação Completa](README.md)
- 🔧 [Exemplos de Expansão](EXEMPLOS_EXPANSAO.md)
- 🌐 [uMod.org](https://umod.org)
- 💬 [Comunidade Rust Brasil](https://discord.gg/rust)

---

**Divirta-se com seu novo plugin!** 🎮✨
