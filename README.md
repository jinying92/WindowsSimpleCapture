# Windows Simple Capture - 便携式截图工具

一个轻量级的Windows截图工具，支持全屏和窗口截图功能。

## 功能特性

- ✅ **便携式设计**: 提供两种发布版本
  - **自包含版本**: 单文件可执行程序（约65MB），包含所有依赖项，无需安装.NET运行时
  - **框架依赖版本**: 超小体积（仅0.71MB），需要预先安装.NET 6运行时
- ✅ **兼容性**: 支持Windows 10及以上版本
- ✅ **窗口置顶**: 可选择强制置于顶层模式
- ✅ **全屏截图**: 一键截取整个屏幕
- ✅ **窗口截图**: 选择特定窗口进行截图
- ✅ **自动隐藏**: 截图时主窗口自动隐藏
- ✅ **智能命名**: 自动按规则命名保存文件

## 使用方法

1. 运行 `WindowsSimpleCapture.exe`
2. 可选择勾选"窗口置于顶层"保持工具窗口始终可见
3. 点击"截取全屏"进行全屏截图
4. 点击"截取窗口"选择特定窗口截图
5. 截图文件自动保存到程序同级目录

## 文件命名规则

- **全屏截图**: `计算机名_时间戳.png`
  - 例: `DESKTOP-ABC123_20231201_143022.png`

- **窗口截图**: `计算机名_窗口名_时间戳.png`
  - 例: `DESKTOP-ABC123_记事本_20231201_143022.png`

## 系统要求

### 自包含版本
- Windows 10 或更高版本
- 无需额外安装.NET运行时

### 框架依赖版本
- Windows 10 或更高版本
- 需要预先安装 [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)
- ✅ **兼容性优势**: .NET 6是LTS版本，兼容性更好，支持更多Win10版本

## 编译方法

### 🚀 快速编译（推荐）

直接运行编译脚本：
```bash
build.bat
```

脚本会提供三个选项：
1. **自包含版本** - 约65MB，无需安装.NET运行时
2. **框架依赖版本** - 仅0.71MB，需要.NET 6运行时
3. **编译两个版本** - 同时生成两种版本

### 📋 单独编译脚本

- **自包含版本**: `build-self-contained.bat`
- **框架依赖版本**: `build-framework-dependent.bat`

### ⚙️ 手动编译命令

**自包含版本**:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**框架依赖版本**:
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

### 📁 输出文件位置

编译后的可执行文件位于：
```
bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe
```

## 开发信息

- 开发语言: C# (.NET 6 LTS)
- UI框架: WPF
- 编译配置: 单文件发布（支持自包含和框架依赖两种模式）