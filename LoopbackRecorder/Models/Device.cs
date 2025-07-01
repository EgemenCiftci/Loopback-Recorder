using NAudio.CoreAudioApi;

namespace LoopbackRecorder.Models
{
    public class Device(MMDevice? mmDevice) : IDisposable
    {
        public MMDevice? MMDevice => mmDevice;

        public AudioClient? AudioClient => MMDevice?.AudioClient;

        public AudioMeterInformation? AudioMeterInformation => MMDevice?.AudioMeterInformation;

        public AudioEndpointVolume? AudioEndpointVolume => MMDevice?.AudioEndpointVolume;

        public AudioSessionManager? AudioSessionManager => MMDevice?.AudioSessionManager;

        public DeviceTopology? DeviceTopology => MMDevice?.DeviceTopology;

        public PropertyStore? Properties => MMDevice?.Properties;

        public string? FriendlyName => MMDevice?.FriendlyName;

        public string? DeviceFriendlyName => MMDevice?.DeviceFriendlyName;

        public string? IconPath => MMDevice?.IconPath;

        public string? InstanceId => MMDevice?.InstanceId;

        public string? ID => MMDevice?.ID;

        public DataFlow? DataFlow => MMDevice?.DataFlow;

        public DeviceState? State => MMDevice?.State;

        public void Dispose()
        {
            MMDevice?.Dispose();
        }
    }
}
