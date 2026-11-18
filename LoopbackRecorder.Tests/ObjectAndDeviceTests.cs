using LoopbackRecorder.Converters;
using LoopbackRecorder.Enums;
using LoopbackRecorder.Models;

namespace LoopbackRecorder.Tests;

[TestClass]
public class ObjectAndDeviceTests
{
    [TestMethod]
    public void ObjectToTypeConverterReturnsRuntimeType()
    {
        ObjectToTypeConverter converter = new();

        object? result = converter.Convert("test", typeof(Type), null!, System.Globalization.CultureInfo.InvariantCulture);

        Assert.AreEqual(typeof(string), result);
    }

    [TestMethod]
    public void ObjectToTypeConverterReturnsNullForNullValue()
    {
        ObjectToTypeConverter converter = new();

        object? result = converter.Convert(null!, typeof(Type), null!, System.Globalization.CultureInfo.InvariantCulture);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ObjectToTypeConverterConvertBackThrows()
    {
        ObjectToTypeConverter converter = new();

        _ = Assert.ThrowsExactly<NotImplementedException>(() =>
            converter.ConvertBack(typeof(string), typeof(object), null!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public void DeviceWithNoNativeDeviceExposesNullProperties()
    {
        using Device device = new(null);

        Assert.IsNull(device.MMDevice);
        Assert.IsNull(device.FriendlyName);
        Assert.IsNull(device.AudioClient);
        Assert.IsNull(device.AudioMeterInformation);
    }

    [TestMethod]
    [DataRow(Formats.Aac)]
    [DataRow(Formats.Mp3)]
    [DataRow(Formats.Wma)]
    public void FormatsEnumParsesSupportedValues(Formats format)
    {
        Assert.IsTrue(Enum.TryParse(format.ToString(), true, out Formats parsed));
        Assert.AreEqual(format, parsed);
    }
}
