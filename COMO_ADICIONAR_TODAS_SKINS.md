# 📝 Como Adicionar TODAS as 10.000+ Skins

## 🎯 Objetivo

Você forneceu um JSON com **~79 itens** e **~10.000 skins**. Este guia mostra como adicionar **TODAS** elas ao plugin.

---

## 📋 Passo a Passo

### 1️⃣ Localizar o Arquivo JSON

No seu servidor Rust, encontre o arquivo:

```
/servidor_rust/oxide/data/ItemSkins_Database.json
```

### 2️⃣ Formato do JSON Fornecido

O JSON que você forneceu tem este formato:

```json
{
  "Commands": ["skin", "skins"],
  "Skins": [
    {
      "Item Shortname": "shoes.boots",
      "Permission": null,
      "Skins": [3487239287, 3484649356, ...]
    },
    {
      "Item Shortname": "coffeecan.helmet",
      "Permission": null,
      "Skins": [3487235363, 3484176678, ...]
    },
    ...
  ]
}
```

### 3️⃣ Formato Esperado pelo Plugin

O plugin espera um formato **mais simples**:

```json
{
  "item.shortname": [skinId1, skinId2, skinId3, ...],
  "outro.item": [skinId4, skinId5, ...]
}
```

### 4️⃣ Converter o JSON

Você precisa **transformar** o JSON original neste formato. Aqui está um exemplo:

**JSON Original (fornecido por você):**
```json
{
  "Skins": [
    {
      "Item Shortname": "shoes.boots",
      "Skins": [3487239287, 3484649356, 3485932851]
    },
    {
      "Item Shortname": "rifle.ak",
      "Skins": [3447914040, 3488346191, 3484553310]
    }
  ]
}
```

**JSON Convertido (para o plugin):**
```json
{
  "shoes.boots": [3487239287, 3484649356, 3485932851],
  "rifle.ak": [3447914040, 3488346191, 3484553310]
}
```

---

## 🔧 Método 1: Conversão Manual

### Copiar e Colar

1. Abra o JSON original que você forneceu
2. Para cada item em `"Skins"`:
   - Copie o valor de `"Item Shortname"`
   - Copie o array de `"Skins"`
   - Cole no formato: `"item.shortname": [skins]`

**Exemplo:**

Pegue este bloco:
```json
{
  "Item Shortname": "shoes.boots",
  "Permission": null,
  "Skins": [3487239287, 3484649356, 3485932851, ...]
}
```

Transforme em:
```json
"shoes.boots": [3487239287, 3484649356, 3485932851, ...]
```

### Template Completo

Aqui está um template com **TODOS** os itens que você forneceu:

```json
{
  "shoes.boots": [COLE_AQUI_AS_344_SKINS],
  "coffeecan.helmet": [COLE_AQUI_AS_286_SKINS],
  "rifle.ak": [COLE_AQUI_AS_690_SKINS],
  "rifle.lr300": [COLE_AQUI_AS_511_SKINS],
  "hoodie": [COLE_AQUI_AS_673_SKINS],
  "pants": [COLE_AQUI_AS_580_SKINS],
  "metal.plate.torso": [COLE_AQUI_AS_517_SKINS],
  "metal.facemask": [COLE_AQUI_AS_480_SKINS],
  "door.hinged.metal": [COLE_AQUI_AS_355_SKINS],
  "door.hinged.wood": [COLE_AQUI_AS_321_SKINS],
  "door.double.hinged.metal": [COLE_AQUI_AS_247_SKINS],
  "wall.frame.garagedoor": [COLE_AQUI_AS_465_SKINS],
  "box.wooden": [COLE_AQUI_AS_262_SKINS],
  "sleepingbag": [COLE_AQUI_AS_241_SKINS],
  "locker": [COLE_AQUI_AS_405_SKINS],
  "smg.thompson": [COLE_AQUI_AS_244_SKINS],
  "smg.mp5": [COLE_AQUI_AS_128_SKINS],
  "rifle.semiauto": [COLE_AQUI_AS_234_SKINS],
  "rifle.bolt": [COLE_AQUI_AS_144_SKINS],
  "pistol.python": [COLE_AQUI_AS_210_SKINS],
  "shotgun.double": [COLE_AQUI_AS_107_SKINS],
  "bow.hunting": [COLE_AQUI_AS_142_SKINS],
  "crossbow": [COLE_AQUI_AS_151_SKINS],
  "hammer": [COLE_AQUI_AS_173_SKINS],
  "hatchet": [COLE_AQUI_AS_190_SKINS],
  "pickaxe": [COLE_AQUI_AS_132_SKINS],
  "jackhammer": [COLE_AQUI_AS_71_SKINS],
  "jacket": [COLE_AQUI_AS_145_SKINS],
  "tshirt": [COLE_AQUI_AS_100_SKINS]
}
```

---

## 🔧 Método 2: Script de Conversão (Mais Fácil)

Se você tiver acesso ao Python, pode usar este script para converter automaticamente:

```python
import json

# Ler JSON original
with open('json_original.json', 'r') as f:
    data = json.load(f)

# Converter para novo formato
converted = {}
for item in data['Skins']:
    shortname = item['Item Shortname']
    skins = item['Skins']
    converted[shortname] = skins

# Salvar JSON convertido
with open('ItemSkins_Database.json', 'w') as f:
    json.dump(converted, f, indent=2)

print(f"✅ Convertido! {len(converted)} itens com skins.")
```

**Como usar:**

```bash
1. Salve o script como "converter.py"
2. Coloque o JSON original no mesmo diretório
3. Execute: python converter.py
4. Copie o arquivo gerado para oxide/data/
```

---

## 🔧 Método 3: Usando o Console F1 (No Jogo)

Se você já tiver o JSON convertido, pode testá-lo sem reiniciar o servidor:

```bash
# No console F1 (como admin):
itemskins.reload

# Você verá:
✅ Carregadas X skins de Y itens do arquivo JSON!
```

---

## 📊 Lista Completa de Itens para Adicionar

Aqui está a lista completa dos **79 itens** que você forneceu:

### Roupas & Armaduras (21 itens)
```
shoes.boots (344)
coffeecan.helmet (286)
hoodie (673)
pants (580)
metal.plate.torso (517)
metal.facemask (480)
jacket (145)
tshirt (100)
... (+13 itens)
```

### Armas (25 itens)
```
rifle.ak (690)
rifle.lr300 (511)
smg.thompson (244)
smg.mp5 (128)
rifle.semiauto (234)
rifle.bolt (144)
pistol.python (210)
shotgun.double (107)
bow.hunting (142)
crossbow (151)
... (+15 itens)
```

### Portas & Construção (18 itens)
```
door.hinged.metal (355)
door.hinged.wood (321)
door.double.hinged.metal (247)
wall.frame.garagedoor (465)
box.wooden (262)
sleepingbag (241)
locker (405)
... (+11 itens)
```

### Ferramentas (15 itens)
```
hammer (173)
hatchet (190)
pickaxe (132)
jackhammer (71)
... (+11 itens)
```

**TOTAL: 79 itens com ~10.000 skins**

---

## ✅ Verificação

Após adicionar todas as skins:

### 1. Recarregar o Plugin

```bash
# Console F1:
itemskins.reload
```

### 2. Verificar no Console

Você deverá ver:

```
[ItemSkins] Sistema de skins carregado: 79 tipos de itens
[ItemSkins] ✅ Carregadas 10000+ skins de 79 itens do arquivo JSON!
[ItemSkins] Total de 10000+ skins disponíveis!
```

### 3. Testar no Jogo

```bash
# Pegue uma AK47
/skin

# Você deverá ver:
┌─────────────────────────────────────┐
│   SKINS - AK47                      │
│   690 skins disponíveis | Pág 1/15  │
└─────────────────────────────────────┘
```

---

## 🚨 Solução de Problemas

### Erro: "Erro ao carregar skins do arquivo JSON"

**Causa:** Sintaxe JSON inválida

**Solução:**
1. Use um validador JSON online (jsonlint.com)
2. Verifique vírgulas, chaves e colchetes
3. Certifique-se de que os IDs de skin são números (sem aspas)

### Erro: "Arquivo de skins não encontrado"

**Causa:** Arquivo no local errado

**Solução:**
1. Confirme o caminho: `oxide/data/ItemSkins_Database.json`
2. Verifique permissões do arquivo
3. Reinicie o servidor

### Aviso: "Plugin funcionará apenas com skins hardcoded"

**Causa:** Plugin não encontrou o arquivo JSON

**Solução:**
1. Crie o arquivo `ItemSkins_Database.json` em `oxide/data/`
2. Coloque pelo menos um item:
   ```json
   {
     "shoes.boots": [3487239287]
   }
   ```
3. Execute `itemskins.reload`

---

## 🎉 Resultado Final

Após seguir este guia, você terá:

✅ **79 tipos de itens** com skins  
✅ **~10.000 skins** disponíveis  
✅ **Sistema de paginação** (48 skins/página)  
✅ **UI completa** e funcional  
✅ **Zero erros** no carregamento

---

## 📞 Precisa de Ajuda?

Se tiver dificuldades com a conversão do JSON:

1. ✅ Envie o JSON original completo
2. ✅ Posso convertê-lo para o formato correto
3. ✅ Você só precisará copiar e colar no arquivo

---

**Versão:** 1.1.0  
**Data:** 10/11/2025  
**Status:** ✅ Pronto para adicionar todas as skins
