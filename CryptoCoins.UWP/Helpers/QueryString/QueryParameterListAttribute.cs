namespace CryptoCoins.UWP.Helpers.QueryString
{
    public class QueryParameterListAttribute : QueryParameterAttribute
    {
        public QueryParameterListAttribute(string key) : base(key)
        {
        }

        public int MaxLength { get; set; } = int.MaxValue;
    }
}
