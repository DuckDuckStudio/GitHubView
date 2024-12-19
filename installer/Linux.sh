# 安装 dpkg
sudo apt-get update
sudo apt-get install -y dpkg
# 创建 DEBIAN 目录和控制文件
mkdir -p output/DEBIAN
echo 'Package: GitHubView' > output/DEBIAN/control
echo 'Version: 1.0' >> output/DEBIAN/control
echo 'Section: base' >> output/DEBIAN/control
echo 'Priority: optional' >> output/DEBIAN/control
echo 'Architecture: all' >> output/DEBIAN/control
#echo 'Maintainer: 鸭鸭「カモ」 <you@example.com>' >> output/DEBIAN/control
echo 'Description: GHV Application' >> output/DEBIAN/control
# 生成安装包
dpkg --build output ghv.deb
