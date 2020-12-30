using Newtonsoft.Json;

namespace Iys.Integration.Console.Extensions {
    public static class ObjectExtensions {
        public static string SerializeToJson<T>(this T serialize) {
            try {
                return JsonConvert.SerializeObject(serialize);
            } catch {
                return string.Empty;
            }
        }
    }
}
