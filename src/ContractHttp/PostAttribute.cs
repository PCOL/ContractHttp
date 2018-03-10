namespace ContractHttp
{
    public class PostAttribute
        : MethodAttribute
    {
        public PostAttribute()
        {
        }

        public PostAttribute(string template)
        {
            this.Template = template;
        }
    }
}