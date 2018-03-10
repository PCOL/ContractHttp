namespace ContractHttp
{
    public class PutAttribute
        : MethodAttribute
    {
        public PutAttribute()
        {
        }

        public PutAttribute(string template)
        {
            this.Template = template;
        }
    }
}