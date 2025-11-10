# 🚀 Guia Rápido - Item Skins

## ⚡ Instalação em 3 Passos

### 1. Upload do Plugin
```
Copie ItemSkins.cs para: /oxide/plugins/ItemSkins.cs
```

### 2. Recarregar
```
oxide.reload ItemSkins
```

### 3. Pronto!
```
Use /skin no jogo!
```

---

## 🎮 Comandos Principais

| Comando | O que faz |
|---------|-----------|
| `/skin` | Abre menu de skins |
| `/skinid 123456` | Aplica skin por ID |
| `/removeskin` | Remove skin |
| `/skinauto` | Skin automática em crafts |
| `/skinfav` | Ver favoritos |

---

## 💡 Uso Rápido

### Aplicar Skin:
1. Segure um item (ex: AK47)
2. Digite `/skin`
3. Clique na skin desejada

### Skin Automática:
1. Aplique uma skin no item
2. Digite `/skinauto`
3. Novos itens craftados terão essa skin!

### Favoritar:
1. Abra `/skin`
2. Clique na ⭐ ao lado da skin
3. Skin será favoritada!

---

## ⚙️ Configuração Básica

Edite: `oxide/config/ItemSkins.json`

```json
{
  "Habilitado": true,
  "Usar permissões": false,
  "Limite de favoritos": 20,
  "Cooldown entre mudanças (segundos)": 1.0
}
```

---

## 🔐 Permissões (Opcional)

Se `"Usar permissões": true`:

```bash
# Dar permissão
oxide.grant user NomeJogador itemskins.use

# Dar para todos
oxide.grant group default itemskins.use
```

---

## 🐛 Problemas?

### "Comando não funciona"
- ✅ Segure um item na mão
- ✅ Verifique permissões
- ✅ Recarregue: `oxide.reload ItemSkins`

### "Skin não aplica"
- ✅ Aguarde 1 segundo (cooldown)
- ✅ Verifique se a skin existe para o item
- ✅ Tente `/removeskin` e aplique novamente

### "Menu não abre"
- ✅ Verifique se o item tem skins
- ✅ Recarregue o plugin
- ✅ Veja erros: `oxide.show errors`

---

## 📊 Itens Populares com Skins

- ✅ AK47 (150+ skins)
- ✅ Thompson (100+ skins)
- ✅ LR-300 (80+ skins)
- ✅ Portas de Metal (50+ skins)
- ✅ Sleeping Bags (40+ skins)
- ✅ Hoodie (200+ skins)
- ✅ Metal Chest Plate (100+ skins)
- ✅ E muito mais!

---

## 🎨 Recursos do UI

- **Estrela dourada (★)** = Skin favoritada
- **Estrela vazia (☆)** = Skin normal
- **Número do botão** = ID da skin
- **Cor laranja** = Skin nos favoritos
- **Cor cinza** = Skin normal

---

## ✅ Checklist de Instalação

- [ ] Plugin copiado para `/oxide/plugins/`
- [ ] Plugin carregado com `oxide.reload ItemSkins`
- [ ] Config criada em `/oxide/config/ItemSkins.json`
- [ ] Permissões configuradas (se necessário)
- [ ] Testado no jogo com `/skin`

---

## 📞 Suporte Rápido

**Plugin não carrega?**
```bash
oxide.plugins
# ItemSkins deve aparecer na lista
```

**Erros de compilação?**
```bash
oxide.show errors
# Verifique se há erros
```

**Ver versão do plugin?**
```bash
oxide.show ItemSkins
```

---

## 🎉 Tudo Pronto!

O plugin está instalado e funcionando!

**Teste agora:**
1. Entre no servidor
2. Pegue uma AK47
3. Digite `/skin`
4. Divirta-se! 🎮

---

**Dúvidas?** Leia o [README.md](README.md) completo!
