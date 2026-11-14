# Hot Virtual Keyboard (Windows 10 / 11 Modern Touch Edition)

> **Industry-Grade Virtual Keyboard with True Windows 10 Touch UI, Adaptive Windows Accent Colors, Voice Typing / Speech Dictation (🎙), Gentle Tactile Audio, 8-Direction Border Drag Resizing, Real Winlogon Lock Screen Desktop Engine, Viewport Clamping, and Modernized Fluent Settings.**

---

## 🌟 Key Features & Improvements

### 1. Voice Input & Speech Dictation (🎙)
- **Direct Microphone Access**:
  - Header toolbar button (`🎙`) alongside layout options.
  - Keyboard keys (`🎙`) embedded directly on the bottom row of both the **Windows 10 Touch Keyboard** and **Full 5-Row Desktop Keyboard**.
- **Windows Voice Typing Engine**: Powered by Windows 10 & 11 native Voice Typing / Dictation (`Win + H`).
- **Live Visual Feedback**: Microphone button illuminates in the Windows Accent Color with a live status banner (`🎙 Listening... Speak now`) and a quick `✕ Stop` button.
- **Settings Integration**: Dedicated "🎙 Test Voice Dictation (Win+H)" preview tool in Settings.

### 2. Adaptive Windows Accent Colors & Instant Tap Highlights
- Dynamically extracts the user's personal Windows Accent Color (`HKCU\Software\Microsoft\Windows\DWM\AccentColor` and `SystemParameters.WindowGlassColor`).
- Tap feedback, button press animations, active modifier states (Shift, Caps, Ctrl, Alt, Win), and toolbar active indicators dynamically light up in your personal Windows theme accent color.
- Modifiers remain illuminated with high-contrast text when active.
- Added interactive **"✨ Test Key Tap Color"** button and live accent swatch inside Settings.

### 3. Auto-Hide on Non-Input Areas
- Global low-level mouse hook (`WH_MOUSE_LL`) detects clicks outside the keyboard window.
- Accurately filters out web documents and non-editable window spaces, automatically collapsing the keyboard when clicking outside editable fields.
- Clicking back into any editable input restores the keyboard immediately.

### 4. True Window Resizing by Dragging Borders (8 Directions)
- Resizes the **entire keyboard window** by grabbing and dragging any of the 4 borders or 4 corners (`HTLEFT`, `HTRIGHT`, `HTTOP`, `HTBOTTOM`, and corners).
- All keys fluidly and proportionally expand/contract in real time.
- Dedicated **Resize Button (`⤡`)** alongside Settings provides:
  - Preset buttons: **Compact (720x250)**, **Standard (860x310)**, **Large (1000x360)**, **Wide (1180x400)**.
  - Quick **"－ Smaller"** and **"＋ Larger"** 10% step buttons.
  - Automatically saves the customized window dimensions.

### 5. Screen Viewport & Taskbar Clamping (No Sinking or Clipping)
- Fixed `ClampToViewport` and `OnWindowSizeChanged` to strictly bind to runtime `ActualWidth` and `ActualHeight`.
- Keeps the floating and docked keyboard completely above the Windows Taskbar and within the primary work area.
- In docked mode, all 5 rows of the Full layout sit visibly above the taskbar without clipping.

### 6. Windows Start Menu & Search Flyout Avoidance
- **Start Menu Occlusion Solved**: Fixed the issue where the virtual keyboard was partially occluded behind the Windows Start Menu when typing into Start search.
- **Smart Screen Avoidance**:
  - In **Floating mode**: When focusing input near the Start Menu / Taskbar search, if screen width permits ($X \ge 690$), it positions side-by-side to the right. If screen width is constrained, it automatically docks to the top of the screen (`Top = workArea.Top + 20`), avoiding any occlusion by the Start flyout.
  - In **Docked Bottom mode**: When Start Menu or Taskbar Search is focused, it automatically shifts to the top edge of the screen so both the Start Menu and the virtual keyboard remain 100% visible with zero overlap.
- **Active Topmost Reassertion**: Reasserts `HWND_TOPMOST` every 500ms and on every outside click/focus event.

### 7. Instant Touch Screen & Mouse Click Detection (30ms) & Auto-Hide
- **Touch Screen Tap Triggers**: Full hardware touch screen and stylus support via `GetLastInputInfo` (`Win32.HasRecentUserInput`). Touching or tapping any input field on touchscreen devices (Surface, tablet, 2-in-1, touch laptop) immediately opens the keyboard.
- **Hardware Mouse Hook Integration**: `GlobalMouseTracker` intercepts clicks at the hardware level and passes physical screen coordinates to `FocusTracker.IsPointOrFocusTextField(clickPoint)`.
- **Zero UI Automation Delay**: Bypasses slow UI Automation focus change queues; detects text boxes, search boxes, and editable web areas directly under the pointer or finger within 30ms.
- **Smart Auto-Hide**: Taps or clicks on non-input whitespace automatically collapse/minimize the keyboard after a 200ms debounce.

### 8. 4 Selectable Visual Themes
- **Windows 10/11 Fluent (Default)**: Modern acrylic dark translucent glass with dynamic Windows Accent Color tap flash.
- **Solid Filled Matte Dark**: 100% opaque matte obsidian styling, solid key tiles, zero transparency, maximum clarity and contrast.
- **Apple Liquid Glass (iOS Frosted Crystal Glass)**: True Apple Liquid Glass aesthetic featuring multi-stop liquid crystal dispersion gradients, specular rim reflections, floating pebble keys with rounded corners (`CornerRadius="7"`), 1px glass bezel, and Apple sapphire blue bloom.
- **Apple Translucent Blurred (Dark Liquid Glass)**: macOS-inspired deep smoked liquid obsidian glass with floating smoky tiles, luminous white key caps, and Monterey azure glow.
- **Quick Switching**: Switch themes on the fly from the keyboard header context menu (`🎨 Keyboard Themes`) or in the modernized Settings dialog (`🎨 Appearance & Size`).

### 9. Modernized Fluent Settings Dialog (Always on Top with Real-Time Effects)
- **Always on Top & Non-Modal**: Opens on top of the keyboard (`Topmost = true`, `Owner = MainWindow`), preventing the dialog from opening behind the keyboard.
- **Draggable Anywhere**: Drag from the title bar or anywhere on the window background. The keyboard stays stationary where it was without hiding or jumping.
- **Real-Time Live Effects**: Every setting change (themes, size presets, docking mode, layout dropdown, repeat sliders, sound toggle) immediately updates the running keyboard live on screen!
- **Modern Controls**: Custom Windows 11 Fluent toggle switches, live audio test, accent color tap test, voice dictation test, and segmented navigation bar.

---

## 🚀 How to Build and Run

### 1. Build
```powershell
cd d:\projects\HotVirtualKeyboard\scripts
.\build.ps1 -Configuration Release
```
Output:
[HotVirtualKeyboard.exe](file:///d:/projects/HotVirtualKeyboard/src/bin/Release/HotVirtualKeyboard.exe)

### 2. Run
```powershell
cd d:\projects\HotVirtualKeyboard\scripts
.\run.ps1
```

### 3. Lock Screen Setup (Administrator)
To install Hot Virtual Keyboard for the Windows Lock Screen and UAC prompts:
1. Open PowerShell as **Administrator**.
2. Run:
```powershell
cd d:\projects\HotVirtualKeyboard\scripts
.\setup_lockscreen.ps1
```
*(Signs the executable with a local certificate, copies it to `Program Files`, and registers it with Windows Accessibility)*.
