# 🌟 Features Completas - Item Skins Plugin

## 🎯 Funcionalidades Principais

### 1. ✨ Sistema de Skins Universal
- **Todas as skins oficiais** do Rust
- **Todas as skins do Workshop** aprovadas
- Suporte para **todos os itens** que possuem skins
- Atualização automática ao adicionar novas skins no Rust

### 2. 🖼️ Interface Gráfica (UI)
- Menu visual intuitivo
- Grid organizado com até 48 skins por tela
- Botões clicáveis para aplicar skins
- Sistema de favoritos visual (⭐)
- Cor customizável
- Responsivo e otimizado

### 3. ⭐ Sistema de Favoritos
- Salvar até 20 skins favoritas (configurável)
- Favoritos destacados em dourado na UI
- Persistência entre reinicializações
- Comando dedicado para gerenciar favoritos
- Indicador visual nas skins favoritadas

### 4. 🔄 Skins Automáticas em Crafts
- Defina uma skin como padrão para um item
- Todos os itens craftados receberão essa skin
- Configurável por tipo de item
- Salvo no perfil do jogador

### 5. 💾 Sistema de Dados Persistentes
- Favoritos salvos em arquivo JSON
- Skins automáticas salvas
- Dados preservados após restart
- Backup automático

### 6. ⚙️ Configuração Completa
- Ativar/desativar plugin
- Controlar acesso às skins
- Sistema de permissões opcional
- Limite de favoritos configurável
- Cooldown customizável
- Cores do UI configuráveis

### 7. 🔐 Sistema de Permissões
- Permissões por usuário ou grupo
- Controle granular de acesso
- Modo livre (sem permissões)
- Permissões administrativas

### 8. ⚡ Performance Otimizada
- Cache de skins em memória
- Cooldown anti-spam
- Limpeza automática de UI
- Salvamento otimizado de dados
- Suporta servidores grandes

---

## 🎮 Comandos Detalhados

### `/skin` - Menu Principal
```bash
/skin
```
**Funcionalidade:**
- Abre interface gráfica com todas as skins
- Mostra skins do item na mão
- Grid com até 48 skins visíveis
- Botões clicáveis para aplicar
- Sistema de favoritos integrado

**Requisitos:**
- Item na mão
- Permissão (se ativada)

---

### `/skins` - Alias de /skin
```bash
/skins
```
**Funcionalidade:**
- Mesmo que `/skin`
- Comando alternativo

---

### `/skinid <ID>` - Aplicar por ID
```bash
/skinid 123456789
```
**Funcionalidade:**
- Aplica skin diretamente pelo ID
- Mais rápido que usar o menu
- Ideal para skins conhecidas

**Exemplo:**
```bash
/skinid 2878289156  # Aplica skin dourada na AK47
```

---

### `/removeskin` - Remover Skin
```bash
/removeskin
```
**Funcionalidade:**
- Remove skin do item na mão
- Volta ao visual padrão
- Instantâneo

---

### `/skinfav [ID]` - Gerenciar Favoritos
```bash
# Ver favoritos
/skinfav

# Adicionar favorito
/skinfav 123456789

# Remover favorito (mesmo comando)
/skinfav 123456789
```
**Funcionalidade:**
- Sem argumentos: lista favoritos
- Com ID: adiciona/remove favorito
- Limite configurável (padrão: 20)
- Toggle on/off

---

### `/skinauto` - Skin Automática
```bash
/skinauto
```
**Funcionalidade:**
- Define skin atual como padrão
- Aplica automaticamente em crafts
- Por tipo de item
- Salvo no perfil

**Exemplo de Uso:**
```bash
1. Craftear AK47
2. Aplicar skin desejada com /skin
3. Digitar /skinauto
4. Todas as próximas AK47 craftadas terão essa skin!
```

---

## 🎨 Sistema de UI Detalhado

### Layout do Menu

```
┌─────────────────────────────────────────────┐
│          SKINS - AK47                       │
│     156 skins disponíveis | Clique para     │
│              aplicar                         │
├─────────────────────────────────────────────┤
│                                             │
│  ☆ 2878289156  ☆ 1234567  ★ 9876543       │
│  ☆ 1111111    ☆ 2222222  ☆ 3333333        │
│  ☆ 4444444    ☆ 5555555  ☆ 6666666        │
│  ...                                        │
│                                             │
├─────────────────────────────────────────────┤
│ [Remover Skin]              [Fechar]       │
└─────────────────────────────────────────────┘
```

### Elementos do UI

**Cabeçalho:**
- Nome do item em maiúsculas
- Contador de skins disponíveis
- Instrução de uso

**Grid de Skins:**
- Até 6 colunas
- Até 8 linhas
- Total: 48 skins por tela
- Espaçamento otimizado

**Cada Botão:**
- ID da skin (se ativado)
- Estrela para favoritar
- Cor indica se é favorito
- Clicável para aplicar

**Rodapé:**
- Botão "Remover Skin" (vermelho)
- Botão "Fechar" (cinza)

### Cores e Indicadores

| Elemento | Cor | Significado |
|----------|-----|-------------|
| ★ Dourada | `#FFD700` | Skin favoritada |
| ☆ Cinza | `#808080` | Skin normal |
| Botão Laranja | `#CC9933` | Skin é favorito |
| Botão Cinza | `#4D4D4D` | Skin normal |
| Botão Vermelho | `#CC3333` | Remover skin |
| Fundo | `#1A1A1A` | Painel principal |

---

## 🔧 Configuração Avançada

### Opções de Performance

```json
{
  "Cooldown entre mudanças (segundos)": 1.0
}
```

**Recomendações:**
- Servidor pequeno (< 50 players): `0.5s`
- Servidor médio (50-100 players): `1.0s`
- Servidor grande (> 100 players): `2.0s`

### Opções de UI

```json
{
  "Mostrar IDs das skins": true,
  "Cor do UI (RGBA)": "0.1 0.1 0.1 0.95"
}
```

**Cores Personalizadas:**
- Preto: `"0.1 0.1 0.1 0.95"`
- Azul escuro: `"0.1 0.1 0.3 0.95"`
- Verde escuro: `"0.1 0.3 0.1 0.95"`
- Vermelho escuro: `"0.3 0.1 0.1 0.95"`

### Opções de Acesso

```json
{
  "Permitir todas as skins": true,
  "Usar permissões": false
}
```

**Modos:**
- Livre: `"Usar permissões": false`
- Restrito: `"Usar permissões": true`
- Híbrido: Permissões por grupo

---

## 📊 Estatísticas e Dados

### Itens com Mais Skins (Aproximado)

| Item | Skins | Categoria |
|------|-------|-----------|
| Hoodie | 250+ | Roupas |
| AK47 | 150+ | Armas |
| Thompson | 120+ | Armas |
| Metal Chest Plate | 100+ | Armadura |
| LR-300 | 80+ | Armas |
| Porta de Metal | 60+ | Construção |
| Sleeping Bag | 50+ | Construção |
| Calça | 200+ | Roupas |
| Botas | 150+ | Roupas |

### Categorias de Skins

1. **Armas** (500+ skins)
   - Rifles, SMGs, Pistolas
   - Espingardas, Arcos
   - Granadas, Explosivos

2. **Roupas** (1000+ skins)
   - Hoodies, Calças, Camisetas
   - Botas, Luvas, Chapéus
   - Máscaras, Óculos

3. **Armaduras** (300+ skins)
   - Metal, Roadsign
   - Coffee Can Helmet
   - Facemasks

4. **Construção** (200+ skins)
   - Portas, Portões
   - Sleeping Bags, Caixas
   - Fornalhas, Workbenches

5. **Decoração** (150+ skins)
   - Placas, Banners
   - Rugs, Painéis
   - Cortinas, Frames

---

## 🚀 Casos de Uso

### 1. Servidor PvP
```json
{
  "Usar permissões": false,
  "Cooldown entre mudanças (segundos)": 0.5,
  "Limite de favoritos": 50
}
```
- Acesso livre para todos
- Cooldown baixo para customização rápida
- Muitos favoritos para diferentes builds

### 2. Servidor Roleplay
```json
{
  "Usar permissões": true,
  "Cooldown entre mudanças (segundos)": 5.0,
  "Limite de favoritos": 10
}
```
- Controle de acesso por grupos
- Cooldown alto para evitar mudanças constantes
- Poucos favoritos para manter imersão

### 3. Servidor Criativo
```json
{
  "Usar permissões": false,
  "Cooldown entre mudanças (segundos)": 0.1,
  "Limite de favoritos": 100
}
```
- Acesso total
- Cooldown mínimo
- Muitos favoritos para criatividade

---

## 🔐 Sistema de Permissões Detalhado

### Hierarquia de Permissões

```
itemskins.all (acesso total)
    │
    ├── itemskins.use (usar comandos)
    │
    └── itemskins.admin (admin)
```

### Exemplos de Configuração

**Setup 1: Livre para Todos**
```bash
# Nenhuma permissão necessária
# Configuração: "Usar permissões": false
```

**Setup 2: VIP e Donators**
```bash
# VIPs tem acesso total
oxide.grant group vip itemskins.all

# Donators tem acesso básico
oxide.grant group donator itemskins.use

# Default não tem acesso
```

**Setup 3: Progressivo**
```bash
# Iniciantes: sem acesso
# Membros: acesso básico
oxide.grant group member itemskins.use

# VIP: acesso total
oxide.grant group vip itemskins.all
```

---

## 💡 Dicas Avançadas

### 1. Organizar Favoritos por Tipo
```bash
# Favoritar skins de armas
AK47 Dourada: 2878289156
LR-300 Preta: 1234567890
Thompson Vermelha: 9876543210

# Criar lista mental ou anotação
```

### 2. Skin Sets (Conjuntos)
```bash
# Criar "set" PvP completo
1. AK47 com skin tática
2. Armadura com skin militar
3. Hoodie com skin camuflado

# Usar /skinauto em cada um
```

### 3. Compartilhar IDs
```bash
# Anotar IDs favoritos e compartilhar
/skinid 2878289156  # AK Dourada
/skinid 1234567890  # LR Tática
```

---

## 📈 Roadmap (Futuras Features)

- [ ] Paginação no UI (para muitas skins)
- [ ] Busca de skins por nome
- [ ] Preview 3D das skins
- [ ] Skin aleatória
- [ ] Coleções de skins
- [ ] Importar/exportar favoritos
- [ ] API para outros plugins
- [ ] Estatísticas de uso

---

## ✅ Compatibilidade com Outros Plugins

### Testado e Compatível

- ✅ **Economics** - Funciona perfeitamente
- ✅ **ServerRewards** - Sem conflitos
- ✅ **Kits** - Skins aplicadas em kits
- ✅ **Clans** - Funciona normalmente
- ✅ **Backpacks** - Itens mantêm skins
- ✅ **CopyPaste** - Construções mantêm skins
- ✅ **Gather Manager** - Sem interferência
- ✅ **Admin Tools** - Compatível

### Possíveis Conflitos

- ⚠️ **Outros plugins de skin** - Pode conflitar
- ⚠️ **Plugins que modificam itens** - Testar antes

---

**Pronto para usar todas as features! 🎉**
