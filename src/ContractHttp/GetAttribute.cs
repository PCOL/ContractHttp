namespace ContractHttp
{
    public class GetAttribute
        : MethodAttribute
    {
        public GetAttribute()
        {
        }

        public GetAttribute(string template)
        {
            this.Template = template;
        }
    }
}