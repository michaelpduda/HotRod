/* This file is part of the HotRod project, which is released under MIT License.
 * See LICENSE.md or visit:
 * https://github.com/michaelpduda/hotrod/blob/master/LICENSE.md
 */

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
