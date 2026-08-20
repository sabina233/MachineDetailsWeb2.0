# 机台图片存放目录

您可以在此目录存放各个机台的实际照片（支持 `.png`, `.jpg`, `.jpeg`, `.svg`, `.webp` 等格式）。

## 使用方式

### 方式一：直接在 `src/config/machineImages.js` 中配置映射
打开 `src/config/machineImages.js`，在 `machineImageMap` 中添加机台名称与图片路径的映射，例如：

```javascript
export const machineImageMap = {
  'RFID-601': '/app/images/machines/RFID-601.png',
  'RFID-602': '/app/images/machines/RFID-602.png',
  'RFID-801': '/app/images/machines/RFID-801.png',
}
```

### 方式二：配置型号/前缀默认图片
如果某个系列机台使用相同的型号外观图，可在 `machineTypeDefaultImages` 中配置：

```javascript
export const machineTypeDefaultImages = {
  'RFID-6': '/app/images/machines/default-roll.png',
  'RFID-8': '/app/images/machines/default-single.png',
}
```

未配置图片或图片无法加载时，系统将自动使用内置的高清矢量设备图标进行优雅展示。
