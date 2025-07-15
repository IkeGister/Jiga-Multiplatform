# JIGA Multiplatform Client – Model Specification

## 1. Overview

The JIGA Multiplatform client is a cross-platform .NET MAUI application serving as a control center for managing and interacting with "Agents"—AI-powered gaming assistants. The app provides agent selection, session management, real-time communication, and live metrics, with a modern, panel-based UI.

---

## 2. Architecture

- **Pattern:** MVVM (Model-View-ViewModel)
- **Dependency Injection:** .NET MAUI DI container
- **Services:** API, WebSocket, Agent Configuration
- **UI:** XAML-based, three-panel layout (Sidebar, Main Content, Right Panel)
- **Platform:** .NET MAUI (C#), designed for cross-platform (Windows, Mac, Android, iOS, etc.)

---

## 3. Data Models

### 3.1 Agent

Represents an AI agent with configuration, capabilities, and UI helper properties.

**Key Properties:**
- `Id`, `Name`, `Description`, `Game`, `VoiceId`, `Nationality`, `Language`
- `Configuration` (see AgentConfiguration)
- `Toolkit` (List of features: e.g., "team_messaging", "learning")
- `AvatarUrl`, `IsActive`, `Bio`
- UI helpers: `DisplayName`, `GameDisplayName`, `IsOnline`, `StatusText`, etc.

**AgentConfiguration:**
- `HighSpeedVision` (bool)
- `HighSpeedMode` (string: "ultra_fast", "balanced", "quality_focused")
- `VoiceEnabled` (bool)
- `AutoConnectAudio` (bool)
- `TargetFps` (double)
- `AnalysisInterval` (double)
- `MaxConcurrentAnalyses` (int)
- `CompressionLevel` (string)
- `EnableCaching` (bool)
- `ExtendedProperties` (Dictionary)

### 3.2 Session

Represents a user’s session with an agent.

**JigaSession:**
- `SessionId`, `UserId`, `Agent`, `Status`, `CreatedAt`
- `Configuration` (see SessionConfiguration)
- `WebSocketEndpoints` (List)
- `Metrics` (SessionMetrics)
- UI helpers: `IsConnected`, `CanConnect`, `StatusDisplayText`, `SessionDuration`, etc.

**SessionConfiguration:**
- `HighSpeedVision`, `HighSpeedMode`, `VoiceChatEnabled`, `AutoConnectAudio`
- `VideoInputSource` (string: "direct_screen", "twitch", "youtube")
- `VideoSourceConfig` (see VideoSourceConfiguration)
- `VoiceChatConfig` (see VoiceChatConfiguration)
- `Permissions` (string: "read_only", "configure", "admin", "full_control")
- `IsAdmin` (bool)
- `ExtendedProperties` (Dictionary)

**VideoSourceConfiguration:**
- `TwitchChannel`, `YouTubeUrl`, `ScreenCaptureConfig` (see ScreenCaptureConfiguration)

**ScreenCaptureConfiguration:**
- `CaptureRegion` (string: "full_screen", "custom_region")
- `CaptureFps` (int)
- `CompressionQuality` (int)

**VoiceChatConfiguration:**
- `VoiceId`, `Nationality`, `Language`, `ResponseType`, `AudioFormat`
- `SampleRate`, `Channels`, `AutoVoiceActivation`, `VoiceActivationThreshold`
- `ContinuousListening`, `MaxRecordingDuration`, `SilenceTimeout`

**SessionMetrics:**
- `FramesProcessed`, `AverageAnalysisTime`, `DetectionsCount`, `ConnectedTime`, `LastActivity`
- `WebSocketMessagesSent/Received`, `AudioMessagesSent/Received`
- UI helpers: `AnalysisTimeText`, `ConnectedTimeText`, `LastActivityText`, `FramesPerSecond`, `FpsText`

### 3.3 API Models

- **SessionRequest/Response:** For creating and managing sessions.
- **SkillToggleRequest:** For toggling agent skills.
- **AudioProcessRequest/Response:** For audio data exchange.
- **WebSocketMessage, FrameMessage, VisionAnalysisResult, Detection, BoundingBox:** For real-time and vision data.

---

## 4. Services

### 4.1 IJigaApiService / JigaApiService

Handles HTTP API communication for session management, agent data, etc.

### 4.2 IWebSocketService / WebSocketService

Manages WebSocket connections for real-time data (metrics, chat, vision, etc.).

### 4.3 IAgentConfigurationService / AgentConfigurationService

Handles agent configuration and session creation requests.

---

## 5. ViewModels

### MainViewModel

- Central orchestrator for agent selection, session management, and UI state.
- Properties: `AvailableAgents`, `SelectedAgent`, `CurrentSession`, `LiveMetrics`, `ConnectionStatus`, etc.
- Commands: `ConnectAgentCommand`, `DisconnectCommand`, `ToggleVoiceChatCommand`, `SelectVideoSourceCommand`, `ExecuteControlActionCommand`, `RefreshAgentsCommand`
- Handles: session creation, WebSocket connection, metrics polling, chat/log updates, error handling.

---

## 6. UI Structure

### 6.1 Main Layout

- **Three-Panel Grid:**
  - **Left Sidebar:** Agent selection (CollectionView of AgentCardControl), logo, collapse button.
  - **Main Content:** Header (selected agent, video source selection, connection status), main body (welcome screen, chat, control panel, activity log, agent status).
  - **Right Panel:** Agent stats, bio, live metrics, configuration (CollapsibleSection controls).

### 6.2 Controls

- **AgentCardControl:** Displays agent info in sidebar.
- **ChatMessageControl:** Displays chat messages.
- **ControlButtonControl:** For agent actions.
- **LogEntryControl:** For activity log.
- **LiveMetricsControl:** For real-time metrics.
- **CollapsibleSection:** Expandable sections in right panel.
- **VideoSourceButton:** For selecting video input source.

### 6.3 Styles

- Custom color scheme, gradients, and styles for a modern, dark-themed UI.

---

## 7. App Initialization

- **App.xaml.cs:** Sets up the main window with MainView, using DI.
- **MauiProgram.cs:** Registers services, viewmodels, and views with the DI container. Configures fonts and logging.

---

## 8. Platform Replication Guidance (iOS)

- **Data Models:** Replicate all C# models as Swift structs/classes or use C# with .NET MAUI for iOS.
- **Services:** Use URLSession/WebSockets in Swift, or reuse C# services in MAUI.
- **MVVM Pattern:** Use SwiftUI’s MVVM or .NET MAUI’s MVVM for iOS.
- **UI:** Map XAML layouts to SwiftUI views, maintaining the three-panel structure and custom controls.
- **Dependency Injection:** Use Swift’s property wrappers or .NET MAUI’s DI.
- **Real-Time:** Use native WebSocket libraries or .NET’s WebSocket support.
- **Theming:** Replicate color schemes and gradients in SwiftUI.
- **Navigation:** Use SwiftUI’s NavigationView/Stack or MAUI’s navigation.

---

## 9. Additional Notes

- **Session and agent management are central.** Ensure robust handling of connection states, errors, and real-time updates.
- **UI is highly dynamic,** with panels showing/hiding based on state (e.g., agent selected, connection status).
- **Metrics and logs** are polled and updated in real time.
- **All models use JSON serialization** for API communication.

---

## 10. References

- See `JIGA-MAUI-Architecture.md` and `JIGA-MAUI-Action-Plan.md` for further architectural and planning details.
- Review all custom controls in the `Controls/` directory for UI replication.

---

**This document provides a complete, factual, and professional specification for replicating the JIGA Multiplatform client on iOS, ensuring fidelity to the original app’s architecture, data models, and user experience.** 