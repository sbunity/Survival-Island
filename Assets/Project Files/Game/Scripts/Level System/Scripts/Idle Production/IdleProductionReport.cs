using System.Text;

namespace Watermelon
{
    public class IdleProductionReport
    {
        public string WorldId { get; set; }
        public float ElapsedMinutes { get; set; }

        private ResourcesList gathered = new ResourcesList();
        public ResourcesList Gathered => gathered;

        private ResourcesList converted = new ResourcesList();
        public ResourcesList Converted => converted;

        public bool IsEmpty => gathered.Count == 0 && converted.Count == 0;

        public void AddGathered(CurrencyType currency, int amount)
        {
            gathered += new Resource(currency, amount);
        }

        public void AddConverted(CurrencyType currency, int amount)
        {
            converted += new Resource(currency, amount);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(WorldId);
            stringBuilder.Append(" — ");
            stringBuilder.Append(ElapsedMinutes.ToString("F1"));
            stringBuilder.Append(" idle min");

            for (int i = 0; i < gathered.Count; i++)
            {
                stringBuilder.Append(" | +");
                stringBuilder.Append(gathered[i].amount);
                stringBuilder.Append(' ');
                stringBuilder.Append(gathered[i].currency);
            }

            for (int i = 0; i < converted.Count; i++)
            {
                stringBuilder.Append(" | converted +");
                stringBuilder.Append(converted[i].amount);
                stringBuilder.Append(' ');
                stringBuilder.Append(converted[i].currency);
            }

            return stringBuilder.ToString();
        }
    }
}
