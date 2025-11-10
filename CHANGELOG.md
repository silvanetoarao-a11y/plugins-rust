# 📝 Changelog - Item Skins Plugin

## Versão 1.0.1 (ATUAL) - Correção de Bug
**Data:** Hoje

### 🐛 Correções
- ✅ **Corrigido erro de compilação** na linha 442
  - Erro: `Type KeyValuePair does not contain a definition for Skinnable`
  - Solução: Corrigido acesso ao Dictionary do Workshop
  - Agora acessa corretamente `.Value.Skinnable`

### 📝 Detalhes Técnicos
```csharp
// ❌ ANTES (ERRO):
var workshopSkins = Rust.Workshop.Approved.All
    .Where(skin => skin.Skinnable?.ItemName == itemDef.shortname)

// ✅ DEPOIS (CORRIGIDO):
foreach (var workshopSkin in Rust.Workshop.Approved.All)
{
    var skinInfo = workshopSkin.Value;
    if (skinInfo != null && skinInfo.Skinnable != null)
    {
        if (skinInfo.Skinnable.ItemName == itemDef.shortname)
```

---

## Versão 1.0.0 - Lançamento Inicial
**Data:** Hoje

### ✨ Funcionalidades
- ✅ Suporte completo a todas as skins do Rust
- ✅ Sistema de skins do Workshop
- ✅ Interface gráfica (UI) intuitiva
- ✅ Sistema de favoritos (até 20 skins)
- ✅ Skins automáticas em crafts
- ✅ Sistema de permissões
- ✅ Configuração customizável
- ✅ Dados persistentes

### 🎮 Comandos
- `/skin` - Menu de skins
- `/skins` - Alias de /skin
- `/skinid <ID>` - Aplicar skin por ID
- `/removeskin` - Remover skin
- `/skinfav [ID]` - Gerenciar favoritos
- `/skinauto` - Skin automática

### ⚙️ Configuração
- Habilitado/Desabilitado
- Permitir todas as skins
- Sistema de permissões opcional
- Limite de favoritos configurável
- Mostrar IDs das skins
- Cor do UI customizável
- Cooldown entre mudanças

### 🔐 Permissões
- `itemskins.use` - Usar plugin
- `itemskins.all` - Acesso total
- `itemskins.admin` - Admin

---

## 🚀 Próximas Versões

### Versão 1.1.0 (Planejada)
- [ ] Sistema de paginação no UI
- [ ] Busca de skins por nome/ID
- [ ] Filtros (por tipo, workshop, oficiais)
- [ ] Skin aleatória
- [ ] Histórico de skins usadas

### Versão 1.2.0 (Planejada)
- [ ] Preview 3D das skins
- [ ] Coleções de skins
- [ ] Compartilhar favoritos
- [ ] Importar/exportar configurações

### Versão 2.0.0 (Futura)
- [ ] Sistema de economia (comprar skins)
- [ ] Skins exclusivas por permissão
- [ ] API para outros plugins
- [ ] Estatísticas de uso
- [ ] Skin marketplace

---

## 📊 Status Atual

### ✅ Funcional
- Todas as funcionalidades principais
- UI completa
- Sistema de favoritos
- Skins automáticas
- Configuração
- Permissões
- Dados persistentes

### 🐛 Bugs Conhecidos
- Nenhum bug conhecido

### ⚠️ Limitações
- Máximo de 48 skins visíveis por tela (paginação em desenvolvimento)
- UI fecha ao morrer (comportamento padrão do Rust)

---

## 🔧 Instruções de Atualização

### De 1.0.0 para 1.0.1

1. **Backup:**
```bash
# Faça backup do arquivo antigo
cp oxide/plugins/ItemSkins.cs oxide/plugins/ItemSkins.cs.backup
```

2. **Substituir:**
```bash
# Substitua pelo novo arquivo
# Copie ItemSkins.cs versão 1.0.1
```

3. **Recarregar:**
```bash
oxide.reload ItemSkins
```

4. **Verificar:**
```bash
oxide.show ItemSkins
# Deve mostrar sem erros
```

### Configuração Preservada
- ✅ Arquivo de config mantido
- ✅ Favoritos dos jogadores mantidos
- ✅ Skins automáticas mantidas
- ✅ Não precisa reconfigurar nada!

---

## 📞 Suporte

### Reportar Bugs
Se encontrar um bug:
1. Anote a mensagem de erro completa
2. Anote o que você estava fazendo
3. Verifique o console: `oxide.show errors`
4. Tente reproduzir o bug

### Verificar Versão
```bash
oxide.show ItemSkins
```

### Logs Úteis
```bash
# Ver erros
oxide.show errors

# Ver plugins
oxide.plugins

# Recarregar
oxide.reload ItemSkins

# Descarregar
oxide.unload ItemSkins
```

---

## ✅ Compatibilidade

### Versões do Rust
- ✅ Protocol 2388.237.1 (Maio 2023)
- ✅ Versões similares

### Oxide/uMod
- ✅ Oxide 2.0.5000+
- ✅ uMod

### Outros Plugins
- ✅ Economics
- ✅ ServerRewards
- ✅ Kits
- ✅ Clans
- ✅ Backpacks
- ✅ E mais!

---

**Última atualização:** Versão 1.0.1
**Status:** ✅ Pronto para produção
