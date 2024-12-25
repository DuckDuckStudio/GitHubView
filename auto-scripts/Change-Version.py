import os
import sys

if len(sys.argv) != 2:
    print("Usage: python Change-Version.py <new_version>")
    sys.exit(1)

new_version = sys.argv[1]
print(f"New version: {new_version}")

program = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))), "ghv", "Program.cs")
try:
    # 读取文件内容
    with open(program, 'r', encoding='utf-8') as f:
        content = f.read()

    # 替换文本
    content = content.replace('Version = "develop"', f'Version = "{new_version}"')

    # 写回文件
    with open(program, 'w', encoding='utf-8') as f:
        f.write(content)
except Exception as e:
    print(f"A error occurred when processing {program}: {e}")
    sys.exit(1)

iss_file = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))), "installer", "Windows.iss")
try:
    # 读取文件内容
    with open(iss_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # 替换文本
    content = content.replace('AppVersion=develop', f'AppVersion={new_version}')

    # 写回文件
    with open(iss_file, 'w', encoding='utf-8') as f:
        f.write(content)
except Exception as e:
    print(f"A error occurred when processing {iss_file}: {e}")
    sys.exit(1)

