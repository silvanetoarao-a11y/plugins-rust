# 🎨 Item Skins - Plugin Completo de Skins para Rust

Plugin completo de gerenciamento de skins para **Rust Protocol 2388.237.1** (Maio/2023)

## ✨ Funcionalidades

- 🎯 **Todas as skins do Rust** - Suporte para TODAS as skins oficiais e do Workshop
- 🖼️ **UI Intuitiva** - Interface gráfica linda e fácil de usar
- ⭐ **Sistema de Favoritos** - Salve suas skins preferidas
- 🔄 **Skin Automática** - Aplique skins automaticamente em itens craftados
- ⚡ **Performance** - Otimizado para servidores grandes
- 🎨 **Customizável** - Configure cores, limites e permissões
- 💾 **Dados Persistentes** - Favoritos salvos entre reinicializações

---

## 📥 Instalação

### 1. Download
Copie o arquivo `ItemSkins.cs` para a pasta:
```
/oxide/plugins/ItemSkins.cs
```

### 2. Carregar Plugin
O plugin será compilado automaticamente, ou use:
```
oxide.reload ItemSkins
```

### 3. Configuração (Opcional)
Após o primeiro carregamento, edite:
```
/oxide/config/ItemSkins.json
```

---

## 🎮 Comandos

### Comandos para Jogadores

| Comando | Descrição | Exemplo |
|---------|-----------|---------|
| `/skin` | Abre menu de skins do item na mão | `/skin` |
| `/skins` | Mesmo que /skin | `/skins` |
| `/skinid <ID>` | Aplica skin por ID | `/skinid 123456` |
| `/removeskin` | Remove skin do item | `/removeskin` |
| `/skinfav [ID]` | Adiciona/remove favorito | `/skinfav 123456` |
| `/skinauto` | Define skin como padrão para crafts | `/skinauto` |

### Como Usar

1. **Aplicar Skin Básico:**
   - Segure um item na mão
   - Digite `/skin`
   - Clique na skin desejada

2. **Aplicar Skin por ID:**
   - Segure um item na mão
   - Digite `/skinid 123456`

3. **Favoritar Skin:**
   - Abra o menu com `/skin`
   - Clique na ⭐ (estrela) da skin
   - Ou use `/skinfav 123456`

4. **Skin Automática em Crafts:**
   - Aplique uma skin no item
   - Digite `/skinauto`
   - Todos os novos itens craftados terão essa skin!

---

## ⚙️ Configuração

### Arquivo: `oxide/config/ItemSkins.json`

```json
{
  "Habilitado": true,
  "Permitir todas as skins": true,
  "Usar permissões": false,
  "Limite de favoritos": 20,
  "Mostrar IDs das skins": true,
  "Cor do UI (RGBA)": "0.1 0.1 0.1 0.95",
  "Cooldown entre mudanças (segundos)": 1.0
}
```

### Explicação das Opções

| Opção | Descrição | Padrão |
|-------|-----------|--------|
| `Habilitado` | Ativa/desativa o plugin | `true` |
| `Permitir todas as skins` | Permite acesso a todas as skins | `true` |
| `Usar permissões` | Requer permissões para usar | `false` |
| `Limite de favoritos` | Máximo de favoritos por jogador | `20` |
| `Mostrar IDs das skins` | Mostra ID nos botões | `true` |
| `Cor do UI (RGBA)` | Cor de fundo da interface | Preto translúcido |
| `Cooldown entre mudanças (segundos)` | Delay entre aplicações | `1.0` |

---

## 🔐 Permissões

### Permissões Disponíveis

| Permissão | Descrição |
|-----------|-----------|
| `itemskins.use` | Permite usar o plugin |
| `itemskins.all` | Acesso total a todas as funcionalidades |
| `itemskins.admin` | Permissões administrativas |

### Como Dar Permissões

```bash
# Dar permissão a um jogador
oxide.grant user <nome> itemskins.use

# Dar permissão a um grupo
oxide.grant group default itemskins.use

# Dar todas as permissões
oxide.grant user <nome> itemskins.all
```

### Remover Permissões

```bash
oxide.revoke user <nome> itemskins.use
```

---

## 🎨 Interface do Usuário (UI)

### Menu Principal

```
╔═══════════════════════════════════════════╗
║     SKINS - AK47                          ║
║     156 skins disponíveis                 ║
╠═══════════════════════════════════════════╣
║                                           ║
║  ☆ 123  ☆ 456  ★ 789  ☆ 012  ☆ 345      ║
║  ☆ 678  ☆ 901  ☆ 234  ☆ 567  ☆ 890      ║
║  ... mais skins ...                       ║
║                                           ║
╠═══════════════════════════════════════════╣
║ [Remover Skin]              [Fechar]     ║
╚═══════════════════════════════════════════╝
```

- **☆** = Skin normal
- **★** = Skin favoritada (dourada)
- **Clique na estrela** = Adicionar/remover favorito
- **Clique no número** = Aplicar skin

---

## 📊 Itens Suportados

O plugin suporta **TODOS os itens** que possuem skins no Rust, incluindo:

### Armas
- AK47, LR-300, Thompson, MP5, Python, SAR, etc.
- Espingardas, Arcos, Bestas
- Granadas, C4, Explosivos

### Ferramentas
- Picareta, Machado, Foice
- Martelo, Plano de Construção

### Roupas
- Hoodie, Calças, Botas
- Capacete, Máscara, Roadsign

### Portas & Construção
- Portas de Metal, Madeira, Garagem
- Sleeping Bags, Caixas, Fornalhas

### Decoração
- Placas, Banners, Rugs
- Painéis, Cortinas

---

## 💡 Dicas e Truques

### 1. Encontrar IDs de Skins
- Abra o menu com `/skin`
- Os IDs são mostrados nos botões
- Anote os IDs das suas favoritas!

### 2. Skins Automáticas
```bash
# Para sempre craftar AK47 com skin dourada:
1. Crafte uma AK47
2. Aplique a skin desejada
3. Digite /skinauto
4. Todas as próximas AK47 terão essa skin!
```

### 3. Gerenciar Favoritos
```bash
# Ver favoritos
/skinfav

# Adicionar favorito
/skinfav 123456

# Remover favorito (usar o mesmo comando)
/skinfav 123456
```

### 4. Remover Todas as Skins
```bash
# Segurar item e usar:
/removeskin
```

---

## 🐛 Solução de Problemas

### Comando não funciona
**Problema:** `/skin` não faz nada

**Soluções:**
1. Verifique se está segurando um item
2. Verifique permissões: `oxide.show perms itemskins`
3. Recarregue: `oxide.reload ItemSkins`

### Skin não aplica
**Problema:** Skin não aparece no item

**Soluções:**
1. Verifique o cooldown (padrão 1 segundo)
2. Verifique se a skin existe para aquele item
3. Tente remover e reaplicar

### UI não abre
**Problema:** Menu não aparece

**Soluções:**
1. Verifique se o item tem skins disponíveis
2. Recarregue o plugin: `oxide.reload ItemSkins`
3. Verifique erros no console: `oxide.show errors`

### Favoritos não salvam
**Problema:** Favoritos desaparecem após reiniciar

**Soluções:**
1. Verifique permissões de escrita da pasta `oxide/data/`
2. Procure o arquivo: `oxide/data/ItemSkins_Data.json`
3. Se não existir, há problema de permissões

---

## 📈 Performance

### Otimizações Incluídas

- ✅ Cache de skins em memória
- ✅ Carregamento assíncrono
- ✅ Cooldown para prevenir spam
- ✅ Limpeza automática de UI ao desconectar
- ✅ Dados salvos apenas quando necessário

### Recomendações

- **Cooldown:** Mínimo 0.5s para servidores grandes
- **Favoritos:** Limite de 20-50 para melhor performance
- **UI:** Fecha automaticamente ao desconectar

---

## 🔄 Compatibilidade

### Versão do Rust
- ✅ Protocol: **2388.237.1**
- ✅ Build Date: **05/04/2023**
- ✅ Compatível com versões similares

### Requisitos
- ✅ Oxide/uMod instalado
- ✅ C# 6.0 (padrão do Oxide)
- ✅ Rust Server atualizado

### Plugins Compatíveis
- ✅ Economics
- ✅ ServerRewards
- ✅ Kits
- ✅ Clans
- ✅ Backpacks

---

## 📝 Changelog

### Versão 1.0.0
- ✨ Lançamento inicial
- ✅ Suporte a todas as skins oficiais
- ✅ Suporte a skins do Workshop
- ✅ Sistema de favoritos
- ✅ Skins automáticas em crafts
- ✅ UI completa e intuitiva
- ✅ Sistema de permissões
- ✅ Configuração customizável
- ✅ Dados persistentes

---

## 🆘 Suporte

### Problemas Comuns

**Q: Posso usar skins premium?**  
A: Sim! Todas as skins aprovadas do Workshop são suportadas.

**Q: As skins funcionam em PvP?**  
A: Sim, são apenas visuais e não afetam o gameplay.

**Q: Posso restringir certas skins?**  
A: Use o sistema de permissões para controlar acesso.

**Q: Funciona com itens customizados de outros plugins?**  
A: Depende do plugin, mas geralmente sim.

---

## 📞 Recursos Adicionais

- 📖 [Documentação Oxide](https://umod.org/documentation)
- 🎮 [Lista de IDs de Skins](https://rustlabs.com/skins)
- 💬 [Comunidade Rust Brasil](https://discord.gg/rust)
- 🔧 [API do Rust](https://developer.valvesoftware.com/wiki/Rust)

---

## 📄 Licença

Este plugin é de código aberto e pode ser modificado livremente.

---

## 🎉 Pronto para Usar!

1. ✅ Copie `ItemSkins.cs` para `oxide/plugins/`
2. ✅ Configure em `oxide/config/ItemSkins.json`
3. ✅ Dê permissões aos jogadores
4. ✅ Divirta-se com as skins!

**Desenvolvido com ❤️ para a comunidade Rust brasileira!** 🇧🇷🎮
