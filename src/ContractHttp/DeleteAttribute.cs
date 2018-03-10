namespace ContractHttp
{
    public class DeleteAttribute
        : MethodAttribute
    {
        public DeleteAttribute()
        {
        }

        public DeleteAttribute(string template)
        {
            this.Template = template;
        }
    }
}