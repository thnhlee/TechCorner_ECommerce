namespace TechCorner_ECommerce.Helpers {
    public static class CodeGenerator {
        public static string Generate(string prefix, int length = 8) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();

            var code = new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
            );

            return $"{prefix}-{code}";
        }
    }
}