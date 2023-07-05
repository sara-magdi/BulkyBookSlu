namespace Di_App.Services
{
    public class SingletonGuidService : ISingletonGuidService
    {
        private readonly Guid Id; 

        public string GetGuid()
        {
            return Id.ToString();
        }
        public SingletonGuidService()
        {
                Id = Guid.NewGuid();    
        }
    }
}
