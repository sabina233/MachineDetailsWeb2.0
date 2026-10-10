# 区域与国旗图片存放目录

本目录用于存放各个国家和区域的国旗或代表性图片。

## 文件命名规范

推荐直接以**区域代码的大写字母命名（.png 或 .jpg / .svg）**，例如：

| 区域代码 | 区域名称 | 推荐文件名 |
|---|---|---|
| `CN` | 中国 | `CN.png` |
| `VN` | 越南 | `VN.png` |
| `US` / `HP` | 美国 / HP厂区 | `US.png` 或 `HP.png` |
| `JP` | 日本 | `JP.png` |
| `KR` | 韩国 | `KR.png` |
| `FR` | 法国 | `FR.png` |
| `DE` / `GER` | 德国 | `DE.png` 或 `GER.png` |
| `GB` / `UK` | 英国 | `GB.png` 或 `UK.png` |
| `IN` / `IND` | 印度 | `IN.png` 或 `IND.png` |
| `GD` | 广东 | `GD.png` |
| `HK` | 香港 | `HK.png` |
| `TW` | 台湾 | `TW.png` |
| `CB` | 柬埔寨 | `CB.png` |
| `ID` | 印尼 | `ID.png` |
| `TH` | 泰国 | `TH.png` |
| `BD` | 孟加拉 | `BD.png` |
| `PK` | 巴基斯坦 | `PK.png` |
| `SLR` | 斯里兰卡 | `SLR.png` |
| `TK` | 土耳其 | `TK.png` |
| `GT` | 危地马拉 | `GT.png` |

## 更多自定义设置

如需使用非 `.png` 格式（如 `.svg`、`.jpg`）或特定文件路径，可直接在项目源码中的：
`src/config/regionImages.js` 中的 `regionImageMap` 进行精确指定。

## 默认显示效果
若本目录中暂未放置某区域的图片，系统会自动展示现代简约风的**矢量国旗/区域徽章（SVG）**，不会显示裂图或空白。
