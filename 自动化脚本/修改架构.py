import os
import sys

def 替换文件内容(文件路径, 原文本, 新文本):
    try:
        # 读取文件内容
        with open(文件路径, 'r', encoding='utf-8') as f:
            内容 = f.read()

        # 替换文本
        内容 = 内容.replace(原文本, 新文本)

        # 写回文件
        with open(文件路径, 'w', encoding='utf-8') as f:
            f.write(内容)
    except Exception as e:
        print(f"[ERROR] 处理 {文件路径} 时出错: {e}")
        sys.exit(1)

if len(sys.argv) != 2:
    print("[ERROR] 使用示例: python xxx.py <架构>")
    sys.exit(1)

架构 = sys.argv[1]
if (not 架构) or (架构 not in ["x86", "x64", "arm", "arm64"]):
    print(f"[ERROR] 架构为空或格式不正确，获取到的架构: {架构}")
    sys.exit(1)
print(f"[INFO] 架构: {架构}")

# 文件路径和替换规则
文件和替换规则 = [
    (os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))), "installer", "DEBIAN", "control"), '这是一个架构', 架构),
    (os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(sys.argv[0]))), "installer", "self-contained", "DEBIAN", "control"), '这是一个架构', 架构)
]

# 执行替换操作
for 文件路径, 原文本, 新文本 in 文件和替换规则:
    替换文件内容(文件路径, 原文本, 新文本)

print("[INFO] 🎉 成功处理所有文件")
sys.exit(0)
