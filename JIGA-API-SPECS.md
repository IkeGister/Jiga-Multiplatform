# JIGA API Specification

## Create Session

The JIGA system provides a comprehensive session creation endpoint that establishes a new JIGA_AI controller session with configurable capabilities and performance settings. This endpoint serves as the entry point for all JIGA interactions and determines the session's processing capabilities, performance characteristics, and feature availability.

### Session Creation Endpoint
**Endpoint:** `/api/v1/jiga/sessions`

This primary endpoint creates a new JIGA_AI controller session with full configuration options. The endpoint accepts user identification, permission settings, and advanced configuration parameters that determine the session's behavior and capabilities. Upon successful creation, the system returns a unique session identifier along with a comprehensive list of available endpoints for that session. The endpoint supports various session types including standard sessions, high-speed vision sessions, and voice chat enabled sessions.

The session creation process includes several key configuration areas:

**Basic Session Configuration:**
- User identification and authentication parameters
- Permission levels and access controls
- Session preferences and customization options
- Administrative privileges and management capabilities

**Video Input Source Configuration:**
- Video input source selection (twitch, youtube, direct_screen)
- Video source configuration parameters (channel name, URL, quality settings)
- Source-specific validation and error handling
- Immutable configuration after session creation

**High-Speed Vision Configuration:**
- Enable or disable high-speed vision processing
- Performance mode selection (balanced, ultra-fast, quality-focused)
- Custom high-speed configuration parameters
- Target frame rates and analysis intervals
- Compression and caching settings

**Voice Chat Configuration:**
- Enable or disable voice chat capabilities
- Automatic audio connection settings
- Voice response configuration including audio format, sample rate, and channels
- Voice activation and noise suppression settings
- Team messaging integration options

**Response and Integration:**
Upon successful session creation, the endpoint returns a comprehensive response including the session identifier, available endpoints for that session, and configuration confirmation. The response includes endpoints for session management, skill configuration, model provider selection, voice settings, team messaging, and various streaming capabilities. The system also provides status endpoints for monitoring session health and performance metrics.

**Automatic Flow Triggering:**
After session creation, the system automatically triggers the next step in the flow based on the configured video input source. For Twitch streams, the system automatically connects to the specified channel and begins frame processing. For YouTube streams, the system prepares for stream processing (when implemented). For direct screen capture, the system prepares the WebSocket endpoints and logs connection information for clients.

**Session Failure Handling:**
The system implements robust error handling for Twitch connection failures. When a Twitch stream cannot be connected after 3 retry attempts with exponential backoff, the session is automatically marked as failed and transitions to an ERROR state. Failed sessions are tracked in metrics and can be queried through dedicated endpoints. Session failure includes comprehensive error logging, state management, and cleanup of partial resources.

### Implementation Details

**Request Model (CreateJigaControllerSessionRequest):**
```python
class CreateJigaControllerSessionRequest(BaseModel):
    user_id: str
    is_admin: bool = False
    permissions: str = "configure"
    preferences: Optional[Dict[str, Any]] = {}
    high_speed_vision: bool = False
    high_speed_mode: str = "balanced"
    high_speed_config: Optional[Dict[str, Any]] = None
    voice_chat_enabled: bool = False
    auto_connect_audio: bool = False
    voice_chat_config: Optional[Dict[str, Any]] = None
    video_input_source: VideoInputSource = Field(
        VideoInputSource.DIRECT_SCREEN, 
        description="Video input source: twitch, youtube, direct_screen"
    )
    video_source_config: Optional[Dict[str, Any]] = Field(
        None, 
        description="Configuration for video source (channel name, URL, quality, etc.)"
    )
```

**Video Input Source Enum:**
```python
class VideoInputSource(str, Enum):
    TWITCH = "twitch"
    YOUTUBE = "youtube"  # Future implementation
    DIRECT_SCREEN = "direct_screen"
```

**Voice Chat Configuration Model:**
```python
class VoiceChatConfig(BaseModel):
    response_type: str = Field("base64_audio", description="Response type: text, audio, base64_audio, team_message")
    audio_format: str = Field("wav", description="Audio format (wav, mp3)")
    sample_rate: int = Field(22050, description="Audio sample rate")
    channels: int = Field(1, description="Audio channels (1=mono, 2=stereo)")
    auto_voice_activation: bool = Field(True, description="Enable automatic voice activation detection")
    voice_activation_threshold: float = Field(0.3, description="Voice activation threshold (0.0-1.0)")
    continuous_listening: bool = Field(True, description="Enable continuous listening mode")
    enable_team_messaging: bool = Field(True, description="Enable team message generation from voice")
    voice_quality: str = Field("standard", description="Voice response quality: low, standard, high")
    enable_noise_suppression: bool = Field(True, description="Enable noise suppression")
    enable_echo_cancellation: bool = Field(True, description="Enable echo cancellation")
    max_recording_duration: int = Field(30, description="Maximum recording duration in seconds")
    silence_timeout: float = Field(2.0, description="Silence timeout before processing (seconds)")
```

**Auto-Trigger Implementation:**
The session creation process automatically calls `_auto_trigger_next_step()` which:
1. Determines the video input source from session configuration
2. Calls appropriate auto-start method based on source type
3. For Twitch: Calls `_auto_start_twitch_streaming()` with retry logic
4. For YouTube: Calls `_auto_start_youtube_streaming()` (future implementation)
5. For Direct Screen: Calls `_auto_prepare_direct_screen_capture()`

**Twitch Connection Retry Logic:**
```python
max_retries = 3
retry_delay = 5.0

for attempt in range(max_retries):
    try:
        await stream_manager.connect()
        # Start frame processing in background
        asyncio.create_task(self._process_twitch_frames(controller_session_id, stream_manager))
        return
    except Exception as e:
        if attempt < max_retries - 1:
            await asyncio.sleep(retry_delay)
            retry_delay *= 1.5  # Exponential backoff
        else:
            await self._fail_session_twitch_connection(controller_session_id, error_msg)
```

**Session Failure Handling:**
When Twitch connection fails after all retries, the system:
1. Updates session state to `ControllerSessionState.ERROR`
2. Updates session metrics with failure information
3. Logs comprehensive failure details
4. Tracks failure in global metrics for monitoring

### Session Types and Capabilities

**Standard Session:**
- Basic JIGA_AI functionality with normal processing speeds
- Standard vision analysis with 3-second intervals
- Text-based responses and basic audio processing
- Direct screen capture as default video input
- Suitable for general gaming assistance and observation

**High-Speed Vision Session:**
- Enhanced processing capabilities with 5x faster frame rates
- Real-time vision analysis with 0.5-second intervals
- Concurrent processing support for multiple simultaneous analyses
- Configurable video input sources (Twitch, YouTube, direct screen)
- Optimized for competitive gaming and real-time scenarios

**Voice Chat Enabled Session:**
- Full voice interaction capabilities
- Real-time audio processing and response generation
- Team messaging and communication features
- Voice activation and noise suppression
- Multiple audio format support
- Compatible with all video input sources

**Combined High-Speed and Voice Session:**
- Maximum performance configuration
- Simultaneous high-speed vision and voice processing
- Real-time multimodal analysis and response generation
- Advanced caching and optimization features
- Full video input source flexibility

### Session Management and Lifecycle

Once created, sessions maintain their configuration throughout their lifetime, with certain parameters being immutable after creation to ensure system stability and performance optimization. Sessions can be configured, monitored, and managed through various additional endpoints, but the core performance characteristics and processing modes are locked in during the initial creation process. The system provides comprehensive session management capabilities including status monitoring, metrics collection, and graceful shutdown procedures.

## Video Capture

The JIGA system provides three primary video input APIs that accept real-time video frame data for analysis and processing. These endpoints support both normal and high-speed vision processing modes, with the high-speed mode offering 5x faster frame rates and 6x more frequent analysis.

### 1. Direct Video Streaming WebSocket
**Endpoint:** `/api/v1/jiga/sessions/{session_id}/streamDirectVideo`

This is the primary video input endpoint that accepts real-time image frames via WebSocket connection. The endpoint automatically detects whether the session is configured for normal or high-speed processing and adjusts its behavior accordingly. When a client connects, it receives a welcome message indicating the processing mode. The endpoint accepts various message types including frame analysis requests, streaming control commands, and heartbeat pings. For frame analysis, clients send base64-encoded JPEG image data along with timestamps, and the system returns comprehensive vision analysis results including detections, confidence scores, and processing metadata. The endpoint supports continuous streaming operations with start and stop commands, and includes robust error handling for invalid messages and processing failures.

### Implementation Details

**WebSocket Connection Flow:**
```python
@app.websocket("/api/v1/jiga/sessions/{jiga_controller_session_id}/streamDirectVideo")
async def jiga_direct_video_streaming_websocket_endpoint(
    websocket: WebSocket, 
    jiga_controller_session_id: str
):
    await websocket.accept()
    
    # Get session context to determine processing mode
    session_context = await get_jiga_session_context(jiga_controller_session_id)
    high_speed_enabled = session_context.get("high_speed_vision", False)
    processing_mode = "high_speed" if high_speed_enabled else "normal"
    
    # Send welcome message with processing mode
    welcome_msg = {
        "type": "direct_video_stream_connected",
        "session_id": jiga_controller_session_id,
        "processing_mode": processing_mode,
        "high_speed_enabled": high_speed_enabled,
        "timestamp": time.time()
    }
    await websocket.send_text(json.dumps(welcome_msg))
```

**Message Processing Loop:**
```python
while True:
    message = await websocket.receive_text()
    data = json.loads(message)
    message_type = data.get("type")
    
    if message_type == "analyze_frame":
        frame_data = data.get("frame_data", "")
        timestamp = data.get("timestamp", time.time())
        
        result = await process_jiga_image_data(
            jiga_controller_session_id,
            frame_data,
            timestamp
        )
        
        await websocket.send_text(json.dumps({
            "type": "direct_video_analysis_result",
            "data": result,
            "processing_mode": processing_mode,
            "timestamp": time.time()
        }))
    
    elif message_type == "ping":
        await websocket.send_text(json.dumps({
            "type": "pong",
            "processing_mode": processing_mode,
            "timestamp": time.time()
        }))
```

**Supported Message Types:**
- `analyze_frame`: Process a single frame with base64-encoded JPEG data
- `start_streaming`: Begin continuous streaming mode
- `stop_streaming`: Stop continuous streaming mode
- `ping`: Heartbeat ping for connection health monitoring

**Response Types:**
- `direct_video_stream_connected`: Initial connection confirmation
- `direct_video_analysis_result`: Frame analysis results
- `streaming_started`: Confirmation of streaming start
- `streaming_stopped`: Confirmation of streaming stop
- `pong`: Response to ping messages
- `error`: Error messages with details

### 2. High-Speed Vision WebSocket
**Endpoint:** `/api/v1/jiga/sessions/{session_id}/high-speed/stream`

This specialized endpoint is optimized for ultra-fast video processing and requires the session to have high-speed vision enabled during creation. The endpoint provides aggressive performance optimizations including result caching, batch processing, and high concurrency handling. When connected, clients receive detailed performance targets and optimization settings including target frame rates, analysis intervals, and concurrent processing limits. The endpoint accepts image data in the same base64 format but processes it with high-speed optimizations, returning analysis results with reduced latency. It also provides real-time metrics and performance monitoring capabilities, allowing clients to track processing speed, cache hit rates, and system performance. The endpoint automatically rejects connections from sessions that don't have high-speed vision enabled, ensuring proper resource allocation.

### Implementation Details

**High-Speed WebSocket Endpoint:**
```python
@app.websocket("/api/v1/jiga/sessions/{jiga_controller_session_id}/high-speed/stream")
async def jiga_high_speed_websocket_endpoint(
    websocket: WebSocket, 
    jiga_controller_session_id: str
):
    await websocket.accept()
    
    # Verify high-speed vision is enabled
    session_context = await get_jiga_session_context(jiga_controller_session_id)
    if not session_context.get("high_speed_vision", False):
        await websocket.close(code=1008, reason="High-speed vision not enabled")
        return
    
    # Send high-speed welcome message with performance targets
    welcome_msg = {
        "type": "high_speed_connected",
        "session_id": jiga_controller_session_id,
        "performance_targets": {
            "target_fps": 5.0,
            "analysis_interval": 0.5,
            "max_concurrent_analyses": 10,
            "compression_level": "high",
            "enable_caching": True
        },
        "timestamp": time.time()
    }
    await websocket.send_text(json.dumps(welcome_msg))
```

**High-Speed Message Processing:**
```python
while True:
    message = await websocket.receive_text()
    data = json.loads(message)
    message_type = data.get("type")
    
    if message_type == "analyze_frame":
        frame_data = data.get("frame_data", "")
        timestamp = data.get("timestamp", time.time())
        
        # High-speed processing with optimizations
        result = await process_jiga_image_data(
            jiga_controller_session_id,
            frame_data,
            timestamp
        )
        
        await websocket.send_text(json.dumps({
            "type": "high_speed_analysis_result",
            "data": result,
            "processing_latency_ms": (time.time() - timestamp) * 1000,
            "timestamp": time.time()
        }))
    
    elif message_type == "get_metrics":
        controller = jiga_session_manager.get_controller(jiga_controller_session_id)
        if controller and hasattr(controller, 'get_high_speed_metrics'):
            metrics = await controller.get_high_speed_metrics()
            await websocket.send_text(json.dumps({
                "type": "high_speed_metrics",
                "data": metrics,
                "timestamp": time.time()
            }))
```

**High-Speed Optimizations:**
- **Batch Processing**: Processes multiple frames concurrently
- **Result Caching**: Caches analysis results for similar frames
- **Compression**: Uses aggressive JPEG compression (30% quality)
- **Concurrent Analysis**: Supports up to 10 simultaneous analyses
- **Performance Monitoring**: Real-time metrics and latency tracking

### 3. Audio Processing with Visual Context
**Endpoints:** 
- `/api/v1/jiga/sessions/{session_id}/audio/process`
- `/api/v1/jiga/sessions/{session_id}/audio/process-with-response`

These endpoints primarily handle audio processing but also accept optional video frame data to provide visual context for audio analysis. When frame data is included, the system performs multimodal analysis combining audio and visual information for more comprehensive understanding. The endpoints support various response types including text, audio, base64-encoded audio, and team messages. The visual context enhances the audio processing by providing situational awareness, allowing the system to generate more relevant and contextual responses. The endpoints include voice chat configuration options and can be configured for different audio formats, sample rates, and processing parameters.

### Implementation Details

**Audio Processing Request Models:**
```python
class AudioDataRequest(BaseModel):
    audio_base64: str = Field(..., description="Base64 encoded audio data")
    frame_data: Optional[str] = Field(None, description="Optional frame data for visual context")
    timestamp: Optional[float] = Field(None, description="Timestamp of the audio")
    audio_format: str = Field("wav", description="Audio format (wav, mp3)")
    sample_rate: int = Field(22050, description="Audio sample rate")
    channels: int = Field(1, description="Audio channels")

class AudioResponseRequest(BaseModel):
    audio_base64: str = Field(..., description="Base64 encoded audio data")
    frame_data: Optional[str] = Field(None, description="Optional frame data for visual context")
    response_type: str = Field("text", description="Response type: text, audio, base64_audio, team_message")
    timestamp: Optional[float] = Field(None, description="Timestamp of the audio")
```

**Audio Processing Endpoints:**
```python
@app.post("/api/v1/jiga/sessions/{jiga_controller_session_id}/audio/process")
async def process_jiga_audio_endpoint(
    jiga_controller_session_id: str,
    request: AudioDataRequest,
    auth: Dict = Depends(verify_token)
):
    result = await process_jiga_audio_data(
        jiga_controller_session_id,
        request.audio_base64,
        request.frame_data,
        request.timestamp
    )
    return result

@app.post("/api/v1/jiga/sessions/{jiga_controller_session_id}/audio/process-with-response")
async def process_jiga_audio_with_response_endpoint(
    jiga_controller_session_id: str,
    request: AudioResponseRequest,
    auth: Dict = Depends(verify_token)
):
    result = await process_jiga_audio_data(
        jiga_controller_session_id,
        request.audio_base64,
        request.frame_data,
        request.timestamp,
        response_type=request.response_type
    )
    return result
```

**Audio WebSocket Streaming:**
```python
@app.websocket("/api/v1/jiga/sessions/{jiga_controller_session_id}/audio/stream")
async def jiga_audio_streaming_websocket(
    websocket: WebSocket, 
    jiga_controller_session_id: str
):
    await websocket.accept()
    
    # Get voice chat configuration
    session_context = await get_jiga_session_context(jiga_controller_session_id)
    voice_chat_enabled = session_context.get("voice_chat_enabled", False)
    auto_connect_audio = session_context.get("auto_connect_audio", False)
    voice_chat_config = session_context.get("voice_chat_config", {})
    
    # Register audio WebSocket in session manager
    jiga_session_manager.register_audio_websocket(jiga_controller_session_id, websocket)
    
    # Send connection confirmation
    connection_data = {
        "type": "connected",
        "message": "JIGA-AI audio streaming WebSocket connected",
        "timestamp": time.time(),
        "session_id": jiga_controller_session_id,
        "voice_chat_enabled": voice_chat_enabled,
        "auto_connect_audio": auto_connect_audio
    }
    await websocket.send_text(json.dumps(connection_data))
```

### Processing Modes and Performance

**Normal Mode:**
- Standard processing with balanced speed and accuracy
- Analysis interval of 3 seconds between frames
- Single-threaded processing with moderate resource usage
- Suitable for general gaming assistance and observation

**High-Speed Mode:**
- Optimized processing achieving up to 5x faster frame rates
- Analysis interval of 0.5 seconds between frames
- Concurrent processing with up to 10 simultaneous analyses
- Aggressive compression and caching for maximum performance
- Suitable for real-time gaming scenarios requiring immediate feedback

### Frame Data Format

All video input APIs accept frame data in a consistent format:
- Base64-encoded JPEG images
- Recommended resolution of 480x270 pixels for optimal performance
- JPEG quality settings of 30-50 for high-speed mode, 50-85 for normal mode
- Timestamp metadata for temporal analysis and synchronization
- Optional source and channel information for multi-stream scenarios

### Error Handling and Reliability

The video capture APIs include comprehensive error handling for various failure scenarios. Invalid frame data is rejected with appropriate error messages, while processing failures are reported with detailed error information. The WebSocket endpoints maintain connection health through ping-pong mechanisms and automatically handle reconnection scenarios. High-speed endpoints include additional safeguards to prevent system overload and maintain performance targets even under heavy load.

## Twitch Stream Capture

The JIGA system provides comprehensive Twitch stream capture capabilities that enable real-time analysis of live gaming streams. This functionality supports direct Twitch integration for automated stream processing, offering efficient deployment options for gaming analysis and competitive scenarios.

### Twitch Integration Architecture

The Twitch stream capture system operates through a multi-layered architecture that connects external Twitch streams to the JIGA vision processing pipeline. The system includes internal components for stream management, frame extraction, and data transformation, as well as external interfaces for client applications to access processed stream data.

**Internal Twitch Stream Manager:**
The system includes a robust TwitchStreamManager component that handles direct connections to Twitch streams using streamlink and PyAV libraries. This component manages stream connections, frame extraction, and data formatting, providing a continuous stream of base64-encoded JPEG frames at configurable frame rates. The manager includes automatic reconnection capabilities, health monitoring, and performance optimization features for both normal and high-speed processing modes.

**Twitch to WebSocket Bridge:**
A specialized bridge component connects the internal Twitch stream manager to the WebSocket vision processing endpoints. This bridge acts as an intermediary that takes raw Twitch stream frames and feeds them into the JIGA vision analysis pipeline, enabling real-time processing of live stream content through internal system processing.

### Implementation Details

**TwitchStreamManager Class:**
```python
class TwitchStreamManager:
    def __init__(
        self, 
        channel_name: str, 
        quality: str = "best", 
        fps: float = 2.0, 
        reconnect_delay: float = 5.0,
        high_speed_mode: bool = False,
        target_resolution: tuple = (480, 270),
        jpeg_quality: int = 50,
        batch_processing: bool = False
    ):
        self.channel_name = channel_name
        self.quality = quality
        self.fps = fps
        self.reconnect_delay = reconnect_delay
        self.high_speed_mode = high_speed_mode
        self.target_resolution = target_resolution
        self.jpeg_quality = jpeg_quality
        self.batch_processing = batch_processing
```

**Stream Connection and Frame Generation:**
```python
async def connect(self):
    if not streamlink or not av:
        raise RuntimeError("Missing dependencies")
    
    twitch_url = f"https://twitch.tv/{self.channel_name}"
    streams = streamlink.streams(twitch_url)
    
    if not streams or self.quality not in streams:
        raise RuntimeError("Stream unavailable")
    
    stream_url = streams[self.quality].url
    self._container = av.open(stream_url)
    self._stream = self._container.streams.video[0]
    self._running = True

async def frame_generator(self) -> AsyncGenerator[str, None]:
    while True:
        try:
            if not self._running:
                await self.connect()
            
            if self.high_speed_mode:
                async for frame_data in self._high_speed_frame_generator():
                    yield frame_data
            else:
                async for frame_data in self._normal_frame_generator():
                    yield frame_data
                    
        except Exception as e:
            logger.error(f"Stream error: {e}. Attempting to reconnect...")
            await self.close()
            await asyncio.sleep(self.reconnect_delay)
            try:
                await self.connect()
            except Exception as conn_e:
                logger.error(f"Reconnect failed: {conn_e}")
                await asyncio.sleep(self.reconnect_delay)
```

**High-Speed Frame Processing:**
```python
async def _high_speed_frame_generator(self) -> AsyncGenerator[str, None]:
    """High-speed frame processing with optimizations."""
    for frame in self._container.decode(self._stream):
        now = time.time()
        if now - self._last_frame_time < 1.0 / self.fps:
            await asyncio.sleep(max(0, 1.0 / self.fps - (now - self._last_frame_time)))
            continue
        
        self._last_frame_time = now
        
        # High-speed optimizations
        pil_image = frame.to_image()
        pil_image = pil_image.resize(self.target_resolution, Image.Resampling.LANCZOS)
        
        # Aggressive compression for high-speed mode
        buffer = io.BytesIO()
        pil_image.save(buffer, format="JPEG", quality=self.jpeg_quality)
        image_bytes = buffer.getvalue()
        base64_image = base64.b64encode(image_bytes).decode('utf-8')
        
        yield base64_image
```

**Auto-Trigger Twitch Streaming:**
```python
async def _auto_start_twitch_streaming(self, controller_session_id: str, video_source_config: Optional[Dict[str, Any]], high_speed_vision: bool):
    if not video_source_config or "channel_name" not in video_source_config:
        await self._fail_session_twitch_connection(controller_session_id, "Missing channel_name")
        return
    
    channel_name = video_source_config["channel_name"]
    
    # Create stream manager with appropriate settings
    stream_manager = TwitchStreamManager(
        channel_name=channel_name,
        quality=video_source_config.get("quality", "best"),
        fps=5.0 if high_speed_vision else 2.0,
        reconnect_delay=5.0,
        high_speed_mode=high_speed_vision,
        target_resolution=(480, 270) if high_speed_vision else (640, 360),
        jpeg_quality=30 if high_speed_vision else 50,
        batch_processing=high_speed_vision
    )
    
    # Store stream manager in session
    self.controller_sessions[controller_session_id].twitch_stream_manager = stream_manager
    
    # Attempt connection with retry logic
    max_retries = 3
    retry_delay = 5.0
    
    for attempt in range(max_retries):
        try:
            await stream_manager.connect()
            # Start frame processing in background
            asyncio.create_task(self._process_twitch_frames(controller_session_id, stream_manager))
            return
        except Exception as e:
            if attempt < max_retries - 1:
                await asyncio.sleep(retry_delay)
                retry_delay *= 1.5  # Exponential backoff
            else:
                await self._fail_session_twitch_connection(controller_session_id, f"Failed after {max_retries} attempts: {e}")
```

**Twitch Frame Processing:**
```python
async def _process_twitch_frames(self, controller_session_id: str, stream_manager):
    controller = self.get_controller(controller_session_id)
    if not controller:
        return
    
    frame_count = 0
    async for frame_data in stream_manager.frame_generator():
        try:
            # Process frame through controller
            result = await controller.process_frame_data(frame_data, time.time())
            
            frame_count += 1
            if frame_count % 10 == 0:
                logger.info(f"Processed {frame_count} frames from Twitch stream")
                
        except Exception as e:
            logger.error(f"Error processing Twitch frame: {e}")
            continue
    finally:
        if hasattr(stream_manager, 'cleanup'):
            await stream_manager.cleanup()
```

**Session Failure Handling:**
```python
async def _fail_session_twitch_connection(self, controller_session_id: str, error_message: str):
    # Update session state to ERROR
    self._update_session_state(controller_session_id, ControllerSessionState.ERROR)
    
    # Update session metrics
    if controller_session_id in self.session_metrics:
        self.session_metrics[controller_session_id].update({
            "failed_operations": self.session_metrics[controller_session_id].get("failed_operations", 0) + 1,
            "last_operation_time": time.time(),
            "twitch_connection_failed": True,
            "twitch_error_message": error_message,
            "session_failed_at": time.time()
        })
    
    # Log comprehensive failure information
    session_info = self.get_controller_session_info(controller_session_id)
    logger.error(f"Session {controller_session_id} failed due to Twitch connection: {error_message}")
```

### Session Creation to Stream Capture Flow

**1. Session Creation with Twitch Configuration:**
The process begins with session creation where users can specify Twitch-related configuration parameters. Sessions can be configured for different stream processing modes, including standard processing for general observation and high-speed processing for competitive gaming scenarios. The session creation endpoint accepts parameters that determine stream quality, frame rates, and processing characteristics.

**2. Stream Source Selection:**
After session creation, the system automatically handles stream capture based on the configured video input source:

**Direct Twitch Integration:**
- Internal system connects directly to Twitch streams
- Automatic frame extraction and processing
- Built-in reconnection and error handling
- Supports multiple quality levels and performance modes
- No client-side processing required

**Direct Screen Capture:**
- Clients capture their own screen content
- Send frames via WebSocket endpoints (`/streamDirectVideo`)
- Full control over capture quality and timing
- Suitable for local gaming analysis
- Enables real-time screen monitoring

**3. Stream Connection and Processing:**
For direct Twitch integration, the system establishes connections to specified Twitch channels and begins frame extraction. The TwitchStreamManager handles stream authentication, quality selection, and frame rate control. Extracted frames are automatically converted to the required base64 JPEG format and fed into the vision processing pipeline.

For direct screen capture, users connect to the appropriate WebSocket endpoints and begin sending frame data. The system processes incoming frames according to the session's configuration and returns analysis results in real-time.

**4. Real-Time Analysis and Response:**
Both capture methods feed into the same vision analysis pipeline, which performs comprehensive game analysis including map location detection, player team identification, event detection, tactical intelligence analysis, and game mode recognition. The system generates real-time callouts, team messages, and audio responses based on the analyzed stream content.

### Stream Capture Methods and Use Cases

**Direct Twitch Integration Use Cases:**
- Automated stream monitoring and analysis
- Competitive gaming assistance without client setup
- Multi-stream analysis and comparison
- Broadcast quality stream processing
- Professional gaming team support

**Direct Screen Capture Use Cases:**
- Local gaming analysis and monitoring
- Real-time screen content processing
- Integration with existing screen capture software
- Privacy-focused local analysis
- Custom quality and timing control

### Performance and Optimization

**Stream Quality Management:**
The system supports various stream quality levels from lowest quality for maximum speed to highest quality for detailed analysis. Quality selection affects both processing speed and analysis accuracy, allowing users to balance performance requirements with analysis precision.

**Frame Rate Control:**
Frame rates are configurable based on session type and processing requirements. Standard sessions typically operate at 1-2 frames per second for general observation, while high-speed sessions can achieve 5+ frames per second for real-time competitive gaming scenarios.

**Connection Reliability:**
The Twitch integration includes robust connection management with automatic reconnection capabilities, health monitoring, and error recovery. The system handles stream interruptions, quality changes, and network issues gracefully to maintain continuous analysis capabilities.

**Resource Management:**
The system includes intelligent resource management that adjusts processing parameters based on available system resources and stream characteristics. This ensures optimal performance while preventing system overload and maintaining analysis quality.

**Connection Failure Handling:**
The Twitch integration includes comprehensive failure handling with automatic retry logic. When connection attempts fail, the system implements a 3-attempt retry strategy with exponential backoff delays (5s, 7.5s, 11.25s). If all retry attempts fail, the session is automatically marked as failed and transitions to an ERROR state. Failed sessions are tracked in system metrics and can be queried through dedicated monitoring endpoints. This ensures that sessions with unreachable Twitch streams are properly handled and don't consume system resources indefinitely.

**Session Failure Recovery:**
Failed sessions due to Twitch connection issues can be identified through session metrics and failure information endpoints. The system provides detailed error messages, failure timestamps, and session state information to assist with troubleshooting and recovery. Failed sessions maintain their configuration and can be recreated with the same parameters once the underlying connection issues are resolved.

## Auto WebSocket Connection

The JIGA system supports automatic WebSocket connection for both audio and vision streaming endpoints. This feature allows sessions to be configured so that, upon creation, the backend will automatically establish and manage WebSocket connections for real-time data streaming, without requiring manual client intervention.

### Configuration

- **auto_connect_audio**: If set to `true` during session creation, the backend will automatically establish an audio WebSocket connection for the session.
- **high_speed_vision**: If enabled, the backend will also auto-connect the high-speed vision WebSocket for direct screen or Twitch video sources.

**Example session creation payload:**
```json
{
  "user_id": "example_user",
  "is_admin": true,
  "permissions": "admin",
  "voice_chat_enabled": true,
  "auto_connect_audio": true,
  "high_speed_vision": true,
  "video_input_source": "direct_screen"
}
```

### Automatic Connection Flow

1. When a session is created with `auto_connect_audio` or `high_speed_vision` enabled, the backend triggers the appropriate auto-connect logic.
2. For audio, the backend establishes a WebSocket connection to `/api/v1/jiga/sessions/{session_id}/audio/stream`.
3. For high-speed vision, the backend connects to `/api/v1/jiga/sessions/{session_id}/high-speed/stream` (if enabled and applicable).
4. The backend manages reconnection, error handling, and session cleanup for these connections.

### Use Cases

- **Hands-free setup**: Users can create sessions that immediately begin streaming audio and/or video data without manual WebSocket connection setup.
- **Automated testing**: Enables integration and system tests to verify real-time streaming features without requiring a client to connect.
- **High-availability gaming**: Ensures that real-time data streams are always available for analysis, even if the client disconnects and reconnects.

### Implementation Notes

- The backend uses async tasks to manage WebSocket connections and reconnections.
- Connection status and errors are tracked in session metrics and logs.
- Auto WebSocket connection is supported for both direct screen and Twitch video sources, as well as for audio streaming when voice chat is enabled.

For more details, see the **Session Creation** and **Video Capture** sections above.
