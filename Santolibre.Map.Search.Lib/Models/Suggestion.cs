namespace Santolibre.Map.Search.Lib.Models
{
    public class Suggestion
    {
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Suggestion item))
            {
                return false;
            }

            return Value.Equals(item.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
