using Newtonsoft.Json;

namespace HotRod
{
    internal static class Extensions
    {
        internal static string ToJson<T>(this T item) =>
            JsonConvert.SerializeObject(item);

        internal static T FromJson<T>(this string json) =>
            JsonConvert.DeserializeObject<T>(json);
    }
}
