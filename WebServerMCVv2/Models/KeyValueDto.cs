namespace WebServerMVCv2.Services
{
    public class KeyValueDto
    {
        //public int Id { get; init; }
        //public string Value { get; init; } = string.Empty;  
        public Dictionary<int, string> KeyValuePairs { get; set; } = new Dictionary<int, string>();
    }
}
