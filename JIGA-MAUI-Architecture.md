# JIGA .NET MAUI Architecture Design

## Overview

This document outlines the architecture for converting the JIGA HTML/JavaScript demo to a production-ready .NET MAUI multiplatform application. The design follows MVVM patterns and implements the features identified from the existing demo.

## Project Structure

```
JigaMultiplatform/
├── Models/              # Data models and DTOs
├── Services/            # Business logic and API communication
├── ViewModels/          # MVVM ViewModels
├── Views/               # XAML UI pages
├── Controls/            # Custom UI controls
├── Converters/          # Value converters for data binding
├── Resources/           # Images, fonts, styles
└── Platforms/           # Platform-specific implementations
```

## Core Architecture Components

### 1. Models Layer

#### Agent Model
```csharp
public class Agent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Game { get; set; }
    public string VoiceId { get; set; }
    public string Nationality { get; set; }
    public string Language { get; set; }
    public AgentConfiguration Configuration { get; set; }
    public List<string> Toolkit { get; set; }
    public string AvatarUrl { get; set; }
}
```

#### Session Management
```csharp
public class JigaSession
{
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public Agent Agent { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public SessionConfiguration Configuration { get; set; }
    public List<string> WebSocketEndpoints { get; set; }
}
```

#### Configuration Models
```csharp
public class SessionConfiguration
{
    public bool HighSpeedVision { get; set; }
    public string HighSpeedMode { get; set; } // ultra_fast, balanced, quality_focused
    public bool VoiceChatEnabled { get; set; }
    public string VideoInputSource { get; set; } // twitch, youtube, direct_screen
    public Dictionary<string, object> ExtendedProperties { get; set; }
}
```

### 2. Services Layer

#### API Communication Service
```csharp
public interface IJigaApiService
{
    Task<JigaSession> CreateSessionAsync(SessionRequest request);
    Task<SessionStatus> GetSessionStatusAsync(string sessionId);
    Task<bool> ConfigureSessionAsync(string sessionId, SessionConfiguration config);
    Task<bool> EnableSkillAsync(string sessionId, string skillName);
    Task<bool> DisableSkillAsync(string sessionId, string skillName);
    Task<SessionMetrics> GetSessionMetricsAsync(string sessionId);
    Task<bool> DeleteSessionAsync(string sessionId);
}
```

#### WebSocket Communication Service
```csharp
public interface IWebSocketService
{
    Task<bool> ConnectAsync(string endpoint, WebSocketType type);
    Task SendFrameAsync(byte[] frameData);
    Task SendAudioAsync(byte[] audioData);
    Task SendMessageAsync(object message);
    Task DisconnectAsync(WebSocketType type);
    
    event EventHandler<WebSocketMessageEventArgs> MessageReceived;
    event EventHandler<WebSocketStatusEventArgs> StatusChanged;
}
```

#### Agent Management Service
```csharp
public interface IAgentService
{
    Task<List<Agent>> GetAvailableAgentsAsync();
    Task<Agent> LoadAgentAsync(string agentId);
    SessionRequest CreateSessionRequest(Agent agent, string userId, SessionConfiguration config);
    List<string> GetAgentCapabilities(Agent agent);
}
```

#### Audio/Video Services
```csharp
public interface IAudioService
{
    Task<bool> StartRecordingAsync();
    Task<byte[]> StopRecordingAsync();
    Task PlayAudioAsync(byte[] audioData);
    bool IsRecording { get; }
}

public interface IVideoService
{
    Task<bool> StartScreenCaptureAsync();
    Task<byte[]> CaptureFrameAsync();
    Task StopScreenCaptureAsync();
    bool IsCapturing { get; }
}
```

### 3. ViewModels Layer (MVVM)

#### Main Application ViewModel
```csharp
public class MainViewModel : BaseViewModel
{
    private readonly IJigaApiService _apiService;
    private readonly IWebSocketService _webSocketService;
    private readonly IAgentService _agentService;
    
    public ObservableCollection<Agent> AvailableAgents { get; set; }
    public Agent SelectedAgent { get; set; }
    public JigaSession CurrentSession { get; set; }
    public SessionMetrics LiveMetrics { get; set; }
    
    public ICommand ConnectAgentCommand { get; set; }
    public ICommand DisconnectCommand { get; set; }
    public ICommand EnableVoiceChatCommand { get; set; }
    public ICommand SendTestFrameCommand { get; set; }
}
```

#### Session Configuration ViewModel
```csharp
public class SessionConfigurationViewModel : BaseViewModel
{
    public string VideoSource { get; set; } // "twitch", "youtube", "direct_screen"
    public string TwitchChannel { get; set; }
    public string YouTubeUrl { get; set; }
    public bool HighSpeedVisionEnabled { get; set; }
    public string HighSpeedMode { get; set; }
    public bool VoiceChatEnabled { get; set; }
    public string SelectedVoiceId { get; set; }
    
    public ICommand CreateSessionCommand { get; set; }
    public ICommand CancelCommand { get; set; }
}
```

### 4. Views Layer (XAML)

#### Main Shell Navigation
```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       x:Class="JigaMultiplatform.AppShell">
    
    <FlyoutItem Title="Agent Selection">
        <ShellContent Route="agents" ContentTemplate="{DataTemplate views:AgentSelectionPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Session Control">
        <ShellContent Route="session" ContentTemplate="{DataTemplate views:SessionControlPage}" />
    </FlyoutItem>
    
    <FlyoutItem Title="Metrics">
        <ShellContent Route="metrics" ContentTemplate="{DataTemplate views:MetricsPage}" />
    </FlyoutItem>
    
</Shell>
```

#### Agent Selection Page Layout
```xml
<ContentPage x:Class="JigaMultiplatform.Views.AgentSelectionPage">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="300" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        
        <!-- Agent Sidebar -->
        <ScrollView Grid.Column="0" BackgroundColor="#1a1a1a">
            <StackLayout Padding="20">
                <Label Text="JIGA AI" StyleClass="HeaderTitle" />
                <CollectionView ItemsSource="{Binding AvailableAgents}"
                               SelectedItem="{Binding SelectedAgent}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <views:AgentCardView />
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>
            </StackLayout>
        </ScrollView>
        
        <!-- Main Content Area -->
        <Grid Grid.Column="1">
            <!-- Session controls, metrics, configuration panels -->
        </Grid>
    </Grid>
</ContentPage>
```

## Data Flow Architecture

### 1. Session Creation Flow
```
User Selects Agent → 
SessionConfigurationViewModel → 
AgentService.CreateSessionRequest → 
ApiService.CreateSessionAsync → 
Update CurrentSession → 
Initialize WebSocket Connections
```

### 2. Real-time Communication Flow
```
Video/Audio Capture → 
WebSocketService.SendFrameAsync → 
Server Processing → 
WebSocket Response → 
Update UI Metrics/Chat
```

### 3. Voice Chat Flow
```
AudioService.StartRecording → 
Capture Audio Data → 
WebSocketService.SendAudioAsync → 
Receive TTS Response → 
AudioService.PlayAudioAsync
```

## Platform-Specific Implementations

### Windows
- Screen capture using Windows APIs
- Audio recording with Windows.Media APIs
- File system access for agent store

### macOS
- Screen capture with macOS permissions
- Audio recording with AVFoundation equivalents
- Menu bar integration

### iOS
- Camera capture for mobile vision features
- Touch-optimized UI layouts
- Push notifications for session updates

### Android
- Camera capture integration
- Touch-optimized UI layouts
- Background service for session management

## Service Registration (Dependency Injection)

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register Services
        builder.Services.AddSingleton<IJigaApiService, JigaApiService>();
        builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
        builder.Services.AddSingleton<IAgentService, AgentService>();
        builder.Services.AddTransient<IAudioService, AudioService>();
        builder.Services.AddTransient<IVideoService, VideoService>();
        
        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<SessionConfigurationViewModel>();
        
        // Register Views
        builder.Services.AddTransient<AgentSelectionPage>();
        builder.Services.AddTransient<SessionControlPage>();
        builder.Services.AddTransient<MetricsPage>();

        return builder.Build();
    }
}
```

## Error Handling Strategy

### 1. Network Errors
- Retry logic for API calls
- Graceful WebSocket reconnection
- User-friendly error messages

### 2. Platform-Specific Errors
- Permission handling for camera/microphone
- Platform capability checks
- Fallback mechanisms

### 3. Session Management Errors
- Session timeout handling
- Recovery from disconnections
- State persistence

## Security Considerations

### 1. API Authentication
- JWT token management
- Secure token storage
- Token refresh handling

### 2. Data Protection
- Sensitive data encryption
- Secure WebSocket connections (WSS)
- User privacy protection

### 3. Platform Security
- Secure credential storage per platform
- Permission management
- Data protection compliance

## Performance Optimizations

### 1. Memory Management
- Proper disposal of WebSocket connections
- Image and audio data cleanup
- ViewModels lifecycle management

### 2. Network Efficiency
- Frame compression for vision streaming
- Audio compression for voice chat
- Efficient JSON serialization

### 3. UI Performance
- Virtual scrolling for large agent lists
- Lazy loading of agent details
- Responsive UI updates

## Testing Strategy

### 1. Unit Tests
- Service layer testing
- ViewModel logic testing
- Model validation testing

### 2. Integration Tests
- API communication testing
- WebSocket connection testing
- Platform-specific feature testing

### 3. UI Tests
- User workflow testing
- Cross-platform UI consistency
- Performance testing

---

This architecture provides a solid foundation for converting the JIGA HTML demo to a production-ready .NET MAUI application while maintaining all the sophisticated features and ensuring excellent cross-platform performance. 