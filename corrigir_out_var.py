#!/usr/bin/env python3
"""
Script para corrigir automaticamente 'out var' para compatibilidade C# 6.0
Uso: python3 corrigir_out_var.py arquivo.cs
"""

import re
import sys
import os

def fix_out_var(content):
    """Corrige out var declarations para C# 6.0"""
    
    # Padrão para detectar 'out var variable'
    pattern = r'\bout\s+var\s+(\w+)\b'
    
    # Encontrar todas as ocorrências
    matches = list(re.finditer(pattern, content))
    
    if not matches:
        print("Nenhum 'out var' encontrado!")
        return content
    
    print(f"Encontradas {len(matches)} ocorrências de 'out var'")
    
    # Processar de trás para frente para não afetar posições
    for match in reversed(matches):
        var_name = match.group(1)
        start = match.start()
        end = match.end()
        
        # Encontrar o início da linha
        line_start = content.rfind('\n', 0, start) + 1
        
        # Obter indentação
        indent = ''
        for char in content[line_start:start]:
            if char in ' \t':
                indent += char
            else:
                break
        
        # Tentar determinar o tipo (difícil sem parser completo)
        # Por padrão, usar 'var' ou tipo específico comum
        tipo = 'var'
        
        # Verificar contextos comuns
        context = content[max(0, start-100):start]
        
        if 'TryParse' in context:
            if 'int.TryParse' in context:
                tipo = 'int'
            elif 'float.TryParse' in context:
                tipo = 'float'
            elif 'double.TryParse' in context:
                tipo = 'double'
            elif 'bool.TryParse' in context:
                tipo = 'bool'
        elif 'TryGetValue' in context:
            # Para Dictionary.TryGetValue, manter var
            tipo = 'var'
        elif 'BasePlayer' in context or 'FindPlayer' in context:
            tipo = 'BasePlayer'
        
        # Criar declaração separada
        declaration = f"{tipo} {var_name};\n{indent}"
        
        # Remover 'var ' do out
        new_out = f"out {var_name}"
        
        # Substituir
        content = content[:line_start] + declaration + content[line_start:start] + new_out + content[end:]
        
        print(f"✓ Corrigido: '{match.group(0)}' → declaração separada com tipo '{tipo}'")
    
    return content

def fix_out_type(content):
    """Corrige out int, out string, etc para C# 6.0"""
    
    # Padrões para tipos específicos
    type_pattern = r'\bout\s+(int|string|float|double|bool|long|ulong|BasePlayer|Vector3)\s+(\w+)\b'
    
    matches = list(re.finditer(type_pattern, content))
    
    if not matches:
        return content
    
    print(f"\nEncontradas {len(matches)} ocorrências de 'out tipo'")
    
    for match in reversed(matches):
        tipo = match.group(1)
        var_name = match.group(2)
        start = match.start()
        end = match.end()
        
        # Encontrar o início da linha
        line_start = content.rfind('\n', 0, start) + 1
        
        # Obter indentação
        indent = ''
        for char in content[line_start:start]:
            if char in ' \t':
                indent += char
            else:
                break
        
        # Criar declaração separada
        declaration = f"{tipo} {var_name};\n{indent}"
        
        # Remover tipo do out
        new_out = f"out {var_name}"
        
        # Substituir
        content = content[:line_start] + declaration + content[line_start:start] + new_out + content[end:]
        
        print(f"✓ Corrigido: '{match.group(0)}' → declaração separada")
    
    return content

def add_cui_using(content):
    """Adiciona using Oxide.Game.Rust.Cui se CuiHelper for usado"""
    
    if 'CuiHelper' not in content:
        return content
    
    if 'using Oxide.Game.Rust.Cui;' in content:
        print("\n'using Oxide.Game.Rust.Cui;' já existe!")
        return content
    
    # Encontrar onde adicionar o using
    using_pattern = r'(using\s+\w+[.\w]*;)'
    matches = list(re.finditer(using_pattern, content))
    
    if matches:
        # Adicionar após o último using
        last_using = matches[-1]
        insert_pos = last_using.end()
        new_using = '\nusing Oxide.Game.Rust.Cui;'
        content = content[:insert_pos] + new_using + content[insert_pos:]
        print("\n✓ Adicionado 'using Oxide.Game.Rust.Cui;'")
    else:
        # Adicionar no início se não houver outros usings
        content = 'using Oxide.Game.Rust.Cui;\n' + content
        print("\n✓ Adicionado 'using Oxide.Game.Rust.Cui;' no início")
    
    return content

def main():
    if len(sys.argv) < 2:
        print("Uso: python3 corrigir_out_var.py arquivo.cs")
        print("\nEste script corrige:")
        print("  - out var → declaração separada")
        print("  - out int/string/etc → declaração separada")
        print("  - Adiciona using Oxide.Game.Rust.Cui se necessário")
        sys.exit(1)
    
    filepath = sys.argv[1]
    
    if not os.path.exists(filepath):
        print(f"Erro: Arquivo '{filepath}' não encontrado!")
        sys.exit(1)
    
    # Criar backup
    backup_path = filepath + '.backup'
    
    print(f"Processando: {filepath}")
    print(f"Criando backup: {backup_path}\n")
    
    # Ler arquivo
    with open(filepath, 'r', encoding='utf-8') as f:
        original = f.read()
    
    # Salvar backup
    with open(backup_path, 'w', encoding='utf-8') as f:
        f.write(original)
    
    # Aplicar correções
    fixed = original
    fixed = fix_out_var(fixed)
    fixed = fix_out_type(fixed)
    fixed = add_cui_using(fixed)
    
    if fixed != original:
        # Salvar arquivo corrigido
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(fixed)
        
        print(f"\n✅ Arquivo corrigido e salvo!")
        print(f"📁 Backup original: {backup_path}")
    else:
        print("\nNenhuma alteração necessária.")
        os.remove(backup_path)

if __name__ == '__main__':
    main()
