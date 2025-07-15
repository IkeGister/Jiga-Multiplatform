using JigaMultiplatform.Models;

namespace JigaMultiplatform.Services;

/// <summary>
/// Service to convert agent configurations to API session requests and WebSocket endpoints.
/// C# equivalent of JavaScript AgentConfigurationInterpreter class.
/// </summary>
public class AgentConfigurationService : IAgentConfigurationService
{
    public SessionRequest ToSessionCreationRequest(
        Agent agent, 
        string userId, 
        VideoInputSource videoSource = VideoInputSource.DirectScreen,
        SessionCustomizations? customizations = null)
    {
        customizations ??= new SessionCustomizations();
        
        return new SessionRequest
        {
            UserId = userId,
            IsAdmin = customizations.IsAdmin ?? false,
            Permissions = customizations.Permissions ?? "configure",
            Preferences = customizations.Preferences ?? new Dictionary<string, object>(),
            
            // High-speed vision configuration
            HighSpeedVision = agent.Configuration.HighSpeedVision,
            HighSpeedMode = agent.Configuration.HighSpeedMode ?? "balanced",
            HighSpeedConfig = ExtractHighSpeedConfigInternal(agent.Configuration),
            
            // Voice chat configuration
            VoiceChatEnabled = agent.Configuration.VoiceEnabled,
            AutoConnectAudio = agent.Configuration.AutoConnectAudio,
            VoiceChatConfig = ExtractVoiceChatConfigInternal(agent, customizations),
            
            // Video input source configuration
            VideoInputSource = videoSource,
            VideoSourceConfig = ExtractVideoSourceConfigInternal(videoSource, customizations)
        };
    }

    public HighSpeedConfiguration ExtractHighSpeedConfig(Agent agent)
    {
        var config = agent.Configuration;
        
        return new HighSpeedConfiguration
        {
            TargetFps = config.TargetFps ?? 3.0,
            AnalysisInterval = config.AnalysisInterval ?? 0.5,
            MaxConcurrentAnalyses = config.MaxConcurrentAnalyses ?? 10,
            CompressionLevel = "high",
            EnableCaching = true
        };
    }

    public VoiceChatConfiguration ExtractVoiceConfig(Agent agent, SessionCustomizations? customizations = null)
    {
        customizations ??= new SessionCustomizations();
        
        return new VoiceChatConfiguration
        {
            ResponseType = "base64_audio",
            AudioFormat = "wav",
            SampleRate = 22050,
            Channels = 1,
            AutoVoiceActivation = true,
            VoiceActivationThreshold = 0.3,
            ContinuousListening = true,
            EnableTeamMessaging = agent.Toolkit.Contains("team_messaging"),
            VoiceQuality = "standard",
            EnableNoiseSuppression = true,
            EnableEchoCancellation = true,
            MaxRecordingDuration = 30,
            SilenceTimeout = 2.0,
            VoiceId = customizations.VoiceId ?? agent.VoiceId,
            Nationality = agent.Nationality,
            Language = agent.Language
        };
    }

    public VideoSourceConfiguration ExtractVideoSourceConfig(VideoInputSource videoSource, SessionCustomizations? customizations = null)
    {
        customizations ??= new SessionCustomizations();
        
        var properties = new Dictionary<string, object>();
        
        switch (videoSource)
        {
            case VideoInputSource.Twitch:
                properties["channel_name"] = customizations.TwitchChannel ?? "example_channel";
                properties["quality"] = customizations.TwitchQuality ?? "best";
                break;
                
            case VideoInputSource.Youtube:
                properties["url"] = customizations.YouTubeUrl ?? "";
                properties["quality"] = customizations.YouTubeQuality ?? "best";
                break;
                
            case VideoInputSource.DirectScreen:
            default:
                properties["resolution"] = "480x270";
                properties["quality"] = 50;
                break;
        }
        
        return new VideoSourceConfiguration
        {
            Source = videoSource,
            Properties = properties
        };
    }

    public WebSocketEndpoints GetWebSocketEndpoints(string sessionId, Agent agent, string baseUrl = "ws://localhost:8000")
    {
        var baseWebSocketUrl = $"{baseUrl}/api/v1/jiga/sessions/{sessionId}";
        var config = agent.Configuration;
        
        var endpoints = new WebSocketEndpoints
        {
            BaseUrl = baseWebSocketUrl,
            Connections = new Dictionary<string, WebSocketConnection>()
        };

        // High-speed vision endpoint (if enabled)
        if (config.HighSpeedVision)
        {
            endpoints.Connections["highSpeed"] = new WebSocketConnection
            {
                Url = $"{baseWebSocketUrl}/high-speed/stream",
                Type = WebSocketType.HighSpeed,
                Priority = 1,
                Enabled = true
            };
        }

        // Standard vision endpoint
        if (agent.Toolkit.Contains("observation"))
        {
            endpoints.Connections["vision"] = new WebSocketConnection
            {
                Url = $"{baseWebSocketUrl}/streamDirectVideo",
                Type = WebSocketType.Vision,
                Priority = 2,
                Enabled = true
            };
        }

        // Voice chat endpoint
        if (config.VoiceEnabled)
        {
            endpoints.Connections["voice"] = new WebSocketConnection
            {
                Url = $"{baseWebSocketUrl}/audio/stream",
                Type = WebSocketType.Audio,
                Priority = 3,
                Enabled = true
            };
        }

        // Team messaging endpoint
        if (agent.Toolkit.Contains("team_messaging"))
        {
            endpoints.Connections["team"] = new WebSocketConnection
            {
                Url = $"{baseWebSocketUrl}/team/stream",
                Type = WebSocketType.TeamMessaging,
                Priority = 4,
                Enabled = true
            };
        }

        return endpoints;
    }

    public List<AgentControlAction> GetControlActions(Agent agent)
    {
        var actions = new List<AgentControlAction>
        {
            // Primary connection button
            new AgentControlAction
            {
                Id = "connect",
                Text = $"Connect {agent.Name}",
                Type = "primary",
                Icon = "🔗",
                Tooltip = $"Connect to {agent.Name} agent"
            }
        };

        // Vision/observation capabilities
        if (agent.Toolkit.Contains("observation"))
        {
            if (agent.Configuration.HighSpeedVision)
            {
                actions.Add(new AgentControlAction
                {
                    Id = "connectVision",
                    Text = "High-Speed Vision",
                    Type = "secondary",
                    Icon = "⚡",
                    Tooltip = "Connect high-speed vision analysis"
                });
            }
            else
            {
                actions.Add(new AgentControlAction
                {
                    Id = "connectVision",
                    Text = "Vision Stream",
                    Type = "secondary",
                    Icon = "👁️",
                    Tooltip = "Connect vision stream"
                });
            }
        }

        // Voice capabilities
        if (agent.HasVoiceChat)
        {
            actions.Add(new AgentControlAction
            {
                Id = "enableVoice",
                Text = "Voice Chat",
                Type = "secondary",
                Icon = "🎤",
                Tooltip = "Enable voice chat communication"
            });
        }

        // Specialist detection
        if (agent.Toolkit.Contains("specialist_detection"))
        {
            actions.Add(new AgentControlAction
            {
                Id = "activateSpecialist",
                Text = "Specialist Mode",
                Type = "secondary",
                Icon = "🎯",
                Tooltip = "Activate specialist game mode detection"
            });
        }

        // Learning capabilities
        if (agent.Toolkit.Contains("learn"))
        {
            actions.Add(new AgentControlAction
            {
                Id = "enableLearning",
                Text = "Learning Mode",
                Type = "secondary",
                Icon = "🧠",
                Tooltip = "Enable adaptive learning mode"
            });
        }

        // Team messaging
        if (agent.Toolkit.Contains("team_messaging"))
        {
            actions.Add(new AgentControlAction
            {
                Id = "enableTeamChat",
                Text = "Team Chat",
                Type = "secondary",
                Icon = "💬",
                Tooltip = "Enable team messaging"
            });
        }

        // Test frame (for observation agents)
        if (agent.Toolkit.Contains("observation"))
        {
            actions.Add(new AgentControlAction
            {
                Id = "sendTestFrame",
                Text = "Test Frame",
                Type = "secondary",
                Icon = "📤",
                Tooltip = "Send test frame for analysis"
            });
        }

        // Disconnect button
        actions.Add(new AgentControlAction
        {
            Id = "disconnect",
            Text = "Disconnect",
            Type = "danger",
            Icon = "🔌",
            Tooltip = "Disconnect from agent"
        });

        return actions;
    }

    public List<MetricConfiguration> GetMetricsConfiguration(Agent agent)
    {
        var metrics = new List<MetricConfiguration>();

        if (agent.Toolkit.Contains("observation"))
        {
            metrics.Add(new MetricConfiguration
            {
                Id = "frameCount",
                Label = "Frames Processed",
                Format = "number",
                DefaultValue = 0
            });

            metrics.Add(new MetricConfiguration
            {
                Id = "analysisTime",
                Label = "Avg Analysis Time",
                Format = "time",
                DefaultValue = "0ms"
            });

            metrics.Add(new MetricConfiguration
            {
                Id = "detectionCount",
                Label = "Detections",
                Format = "number",
                DefaultValue = 0
            });
        }

        if (agent.HasVoiceChat)
        {
            metrics.Add(new MetricConfiguration
            {
                Id = "responseTime",
                Label = "Response Time",
                Format = "time",
                DefaultValue = "0ms"
            });

            metrics.Add(new MetricConfiguration
            {
                Id = "voiceInteractions",
                Label = "Voice Interactions",
                Format = "number",
                DefaultValue = 0
            });
        }

        if (agent.Toolkit.Contains("learn"))
        {
            metrics.Add(new MetricConfiguration
            {
                Id = "learningEvents",
                Label = "Learning Events",
                Format = "number",
                DefaultValue = 0
            });

            metrics.Add(new MetricConfiguration
            {
                Id = "adaptations",
                Label = "Adaptations",
                Format = "number",
                DefaultValue = 0
            });
        }

        if (agent.Toolkit.Contains("specialist_detection"))
        {
            metrics.Add(new MetricConfiguration
            {
                Id = "specialistDetections",
                Label = "Specialist Detections",
                Format = "number",
                DefaultValue = 0
            });

            metrics.Add(new MetricConfiguration
            {
                Id = "tacticalAdvice",
                Label = "Tactical Advice",
                Format = "number",
                DefaultValue = 0
            });
        }

        // Always include these basic metrics
        metrics.Add(new MetricConfiguration
        {
            Id = "actions",
            Label = "Actions Taken",
            Format = "number",
            DefaultValue = 0
        });

        metrics.Add(new MetricConfiguration
        {
            Id = "connectionTime",
            Label = "Connected Time",
            Format = "time",
            DefaultValue = "0s"
        });

        metrics.Add(new MetricConfiguration
        {
            Id = "uptime",
            Label = "Session Uptime",
            Format = "time",
            DefaultValue = "0s"
        });

        return metrics;
    }

    // Internal helper methods
    private Dictionary<string, object> ExtractHighSpeedConfigInternal(AgentConfiguration config)
    {
        return new Dictionary<string, object>
        {
            ["target_fps"] = config.TargetFps ?? 3.0,
            ["analysis_interval"] = config.AnalysisInterval ?? 0.5,
            ["max_concurrent_analyses"] = config.MaxConcurrentAnalyses ?? 10,
            ["compression_level"] = "high",
            ["enable_caching"] = true
        };
    }

    private Dictionary<string, object> ExtractVoiceChatConfigInternal(Agent agent, SessionCustomizations customizations)
    {
        return new Dictionary<string, object>
        {
            ["response_type"] = "base64_audio",
            ["audio_format"] = "wav",
            ["sample_rate"] = 22050,
            ["channels"] = 1,
            ["auto_voice_activation"] = true,
            ["voice_activation_threshold"] = 0.3,
            ["continuous_listening"] = true,
            ["enable_team_messaging"] = agent.Toolkit.Contains("team_messaging"),
            ["voice_quality"] = "standard",
            ["enable_noise_suppression"] = true,
            ["enable_echo_cancellation"] = true,
            ["max_recording_duration"] = 30,
            ["silence_timeout"] = 2.0,
            ["voice_id"] = customizations.VoiceId ?? agent.VoiceId,
            ["nationality"] = agent.Nationality,
            ["language"] = agent.Language
        };
    }

    private Dictionary<string, object> ExtractVideoSourceConfigInternal(VideoInputSource videoSource, SessionCustomizations customizations)
    {
        return videoSource switch
        {
            VideoInputSource.Twitch => new Dictionary<string, object>
            {
                ["channel_name"] = customizations.TwitchChannel ?? "example_channel",
                ["quality"] = customizations.TwitchQuality ?? "best"
            },
            VideoInputSource.Youtube => new Dictionary<string, object>
            {
                ["url"] = customizations.YouTubeUrl ?? "",
                ["quality"] = customizations.YouTubeQuality ?? "best"
            },
            VideoInputSource.DirectScreen => new Dictionary<string, object>
            {
                ["resolution"] = "480x270",
                ["quality"] = 50
            },
            _ => new Dictionary<string, object>
            {
                ["resolution"] = "480x270",
                ["quality"] = 50
            }
        };
    }
} 