namespace ContractHttp
{
    public class PatchAttribute
        : MethodAttribute
    {
        public PatchAttribute()
        {
        }

        public PatchAttribute(string template)
        {
            this.Template = template;
        }
    }
}