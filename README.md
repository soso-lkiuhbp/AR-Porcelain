# AR-Porcelain —— AR叙事解谜互动游戏

Unity 2022.3.43f1c1 + Vuforia Engine 开发的 AR 叙事解谜互动游戏，独立负责全部游戏逻辑与交互系统开发。AR 图像识别作为玩法入口，结合拼图解谜玩法与多节点叙事驱动系统，实现 6 场景完整流程。

## 技术栈

Unity 2022.3.43f1c1 · Vuforia Engine · C# · Unity UI · TextMeshPro

---

## 核心系统

### 1. Vuforia 识别回调封装（MyObserverEventHandler）

- 自定义 `ITrackableEventHandler` 实现，处理识别成功/丢失/重新识别等状态
- **位姿平滑过渡**：`PoseSmoother` 类实现识别位姿平滑过渡动画（0.3秒 Lerp）
- 识别成功调用 `ARModelController.instance.OnTargetFound()`
- 识别丢失调用 `ARModelController.instance.OnTargetLost()`

### 2. 拼图解谜玩法（PuzzleManager / PuzzlePiece / Slot）

- **9宫格拼图**随机打乱（`CreatePieces()` 使用 Fisher-Yates 洗牌算法）
- 拖放吸附校验（`TryPlacePiece()`）：正确放置 `correctCount++`，错误播放 `wrongSound`
- 完整拼合后触发**剧情演出**（`OnPuzzleComplete()`）与**场景切换**（`LoadNextScene()`）

### 3. 叙事驱动系统（StoryManager）

- **逐字显示**剧情文字（`TypeSentence()` 协程）
- 每句话之间延迟（`delayBetweenLines`）
- 所有句子播放完毕自动**跳转场景**（`SceneManager.LoadScene()`）

### 4. 3D 模型触摸交互（ARModelController）

- 识别成功后显示 3D 瓷器模型
- 支持触摸**旋转**与**缩放**

---

## 游戏流程

