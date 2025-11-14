# Unity Safe Area Manager

A comprehensive and advanced solution for handling device safe areas (notches, rounded corners, etc.) in Unity UGUI with flexible positioning strategies, smooth animations, and extensive customization options.

![Unity Version](https://img.shields.io/badge/Unity-2019.4+-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)
[![GitHub issues](https://img.shields.io/github/issues/yourusername/unity-safe-area.svg)](https://github.com/yourusername/unity-safe-area/issues)

## ✨ Features

- **🎯 Multiple Positioning Strategies**: Stretch, Fit, Custom Margins, Individual Sides Control
- **🎨 Advanced Custom Inspector**: Visual preview, device simulation, intuitive controls
- **🔄 Smooth Animations**: Configurable transitions when safe area changes
- **📱 Device Simulation**: Test different devices directly in the editor
- **🔧 Post-Processing Pipeline**: Chain multiple post-processors for complex layouts
- **⚡ Performance Optimized**: Smart update detection and caching
- **🎭 Visual Feedback**: Real-time preview in Scene view
- **📚 Comprehensive API**: Full programmatic control

## 🚀 Quick Start

### Basic Setup

1. Add a `SafeArea` component to any UI element that needs to respect safe areas
2. Ensure your Canvas has a `CanvasScaler` component
3. Configure the positioning strategy in the inspector

```csharp
// Programmatic setup
var safeArea = gameObject.AddComponent<SafeArea>();
safeArea.Strategy = SafeArea.PositioningStrategy.Stretch;
safeArea.UpdateSafeArea();
```

### Common Use Cases

#### Stretch to Fill Safe Area
```csharp
safeArea.Strategy = SafeArea.PositioningStrategy.Stretch;
// Content will fill the entire safe area
```

#### Fit with Aspect Ratio
```csharp
safeArea.Strategy = SafeArea.PositioningStrategy.Fit;
// Content maintains aspect ratio within safe area
```

#### Custom Margins
```csharp
safeArea.Strategy = SafeArea.PositioningStrategy.CustomMargins;
safeArea.LeftMargin = 20f;
safeArea.RightMargin = 20f;
safeArea.TopMargin = 40f;
safeArea.BottomMargin = 20f;
```

#### Individual Side Control
```csharp
safeArea.Strategy = SafeArea.PositioningStrategy.IndividualSides;
safeArea.TopSide = new SafeArea.SafeAreaSide(true, 50f); // Notch area
safeArea.BottomSide = new SafeArea.SafeAreaSide(false, 0f); // Ignore bottom
```

## 📖 API Reference

### SafeArea Component

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Strategy` | `PositioningStrategy` | Positioning mode (Stretch, Fit, CustomMargins, IndividualSides) |
| `Padding` | `float` | Additional padding around the safe area |
| `CanvasSafeArea` | `Rect` | Current safe area in canvas coordinates (read-only) |
| `HasSafeAreaChanged` | `bool` | Whether safe area changed since last update (read-only) |

#### Methods

| Method | Description |
|--------|-------------|
| `UpdateSafeArea()` | Force update safe area positioning |
| `ForceRefresh()` | Reset all cached values and recalculate |
| `GetSafeAreaMargins()` | Get margins as Vector4 (left, top, right, bottom) |
| `IsPointInSafeArea(Vector2)` | Check if point is within safe area |

#### Positioning Strategies

- **Stretch**: Fills entire safe area
- **Fit**: Maintains aspect ratio, centers within safe area
- **CustomMargins**: Fixed margins from screen edges
- **IndividualSides**: Control each side independently with enable/disable options

### Post-Processors

#### MaxWidthSafeAreaPostProcess
Limits maximum width relative to height.

```csharp
var maxWidth = gameObject.AddComponent<MaxWidthSafeAreaPostProcess>();
maxWidth.MaxWidthRatio = 2.0f; // Max 2:1 aspect ratio
maxWidth.Mode = MaxWidthSafeAreaPostProcess.WidthLimitMode.Center;
```

#### AspectRatioSafeAreaPostProcess
Enforces aspect ratio constraints.

```csharp
var aspectRatio = gameObject.AddComponent<AspectRatioSafeAreaPostProcess>();
aspectRatio.MaxAspectRatio = 2.1f;
aspectRatio.MinAspectRatio = 0.5f;
aspectRatio.Mode = AspectRatioSafeAreaPostProcess.AspectRatioMode.ScaleToFit;
```

#### PaddingSafeAreaPostProcess
Adds configurable padding around content.

```csharp
var padding = gameObject.AddComponent<PaddingSafeAreaPostProcess>();
padding.SetUniformPadding(20f);
// Or set individual sides
padding.SetPadding(10f, 20f, 10f, 30f);
```

## 🎨 Custom Inspector Features

### Visual Preview
- Real-time safe area visualization in Scene view
- Customizable colors for safe/unsafe areas
- Device frame simulation

### Device Simulation
Test your UI on different devices:
- iPhone SE, iPhone 14, iPhone 14 Pro, iPhone 14 Pro Max
- iPad, iPad Pro
- Samsung Galaxy S23, Pixel 7
- Custom resolutions

### Interactive Controls
- Foldable sections for organization
- Visual margin editor for CustomMargins strategy
- Quick-add buttons for common post-processors

## 🔧 Advanced Usage

### Animation Setup
```csharp
safeArea.AnimateChanges = true;
safeArea.AnimationDuration = 0.5f;
safeArea.AnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
```

### Post-Processing Pipeline
Post-processors are applied in order and can be chained:

```csharp
// Add multiple post-processors to the same GameObject
gameObject.AddComponent<AspectRatioSafeAreaPostProcess>();
gameObject.AddComponent<PaddingSafeAreaPostProcess>();
gameObject.AddComponent<MaxWidthSafeAreaPostProcess>();
```

### Custom Post-Processor
```csharp
public class CustomPostProcessor : SafeAreaPostProcess
{
    public override void PostProcess(SafeArea safeArea)
    {
        // Your custom logic here
        var pos = safeArea.RectTransform.anchoredPosition;
        var size = safeArea.RectTransform.sizeDelta;

        // Modify position/size as needed

        safeArea.RectTransform.anchoredPosition = pos;
        safeArea.RectTransform.sizeDelta = size;
    }
}
```

## 📱 Supported Platforms

- **iOS**: Full support for notches, Dynamic Island, rounded corners
- **Android**: Support for notches, punch-holes, rounded corners
- **WebGL**: Graceful fallback (safe area not available)
- **Other Platforms**: Uses Screen.safeArea when available

## ⚙️ CanvasScaler Compatibility

Works with all CanvasScaler modes:
- **MatchWidthOrHeight**: Automatic scaling calculations
- **Expand**: Full canvas expansion support
- **Shrink**: Canvas shrinking support
- **ConstantPixelSize**: Direct pixel mapping

## 🚀 Performance Considerations

- Safe area updates are optimized to run only when necessary
- Screen size changes trigger immediate updates
- Safe area changes are checked with floating-point tolerance
- Editor-only code is stripped in builds

## 🐛 Troubleshooting

### Common Issues

**Safe area not updating:**
- Ensure CanvasScaler is present in parent hierarchy
- Check that CanvasScaler has valid reference resolution
- Try calling `ForceRefresh()` manually

**Incorrect positioning:**
- Verify Canvas render mode (Screen Space Camera/Overlay)
- Check RectTransform anchor/pivot settings are reset by SafeArea
- Ensure no conflicting layout components

**Editor preview not showing:**
- Enable Gizmos in Scene view
- Check that SafeArea component is enabled
- Try refreshing the Scene view

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup

1. Clone the repository
2. Open in Unity 2019.4+
3. The package is editor-ready with all dependencies included

### Testing

Test on multiple devices and CanvasScaler configurations to ensure compatibility.

## 📞 Support

- Create an [issue](https://github.com/yourusername/unity-safe-area/issues) for bugs
- Check the [Wiki](https://github.com/yourusername/unity-safe-area/wiki) for guides
- Join the [Discussions](https://github.com/yourusername/unity-safe-area/discussions) for questions

---

Made with ❤️ for Unity developers
