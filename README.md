# 智商不够别点 —— 游戏棋盘视觉识别工具

基于 F# + OpenCVSharp + Win32 API 的窗口截屏与分析工具
自动捕获指定窗口的游戏区域，专为微信小游戏《智商不够别点》设计

---

## ✨ 项目特性

- **函数式编程风格**：采用管道式数据流（Captured → Grayed → Thresholded → Contoured → ...），代码清晰、类型安全、可组合性强
- **自动窗口管理**：支持查找指定窗口标题、自动恢复最小化窗口
- **精准 ROI 截屏**：根据预设比例动态计算游戏主区域，避免截取多余部分
- **完整图像处理管道**：
    - 灰度转换
    - 二值化（BinaryInv）
    - 外部轮廓提取
    - 中心点计算
    - 按列智能分组 + 行列匹配验证

---

## 🚀 快速开始

### 前置要求

- **使用 .net10.0-windows SDK 开发，其他版本兼容性为止**
- Windows 系统（因使用 Win32 API 和 System.Drawing）
- JetBrains Rider（推荐）或 Visual Studio
- NuGet 依赖（项目已引用）：
    - `OpenCvSharp4`
    - `OpenCvSharp4.runtime.win`
    - `System.Drawing.Common`

### 运行步骤

```bash
# 1. 克隆仓库
git clone https://github.com/Msenxy/AutoPlayGame.git
cd AutoPlayGame

# 2. 恢复与构建
dotnet restore
dotnet build

# 3. 运行程序
dotnet run
```
