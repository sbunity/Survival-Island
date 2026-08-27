using System.Text;

namespace Watermelon
{
    public static class TraderResourceFormat
    {
        public static string Format(Resource[] resources)
        {
            if (resources == null || resources.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();

            for (var i = 0; i < resources.Length; i++)
            {
                if (i > 0)
                    builder.Append(' ');

                Append(builder, resources[i]);
            }

            return builder.ToString();
        }

        public static string Format(Resource resource)
        {
            var builder = new StringBuilder();

            Append(builder, resource);

            return builder.ToString();
        }

        private static void Append(StringBuilder builder, Resource resource)
        {
            builder.Append("<sprite name=").Append(resource.currency).Append('>').Append(resource.amount);
        }
    }
}
