
namespace COM.IBM.ML.ILOG
{
    public interface TokenHandler
    {
        /* Inits the token authenticator */
        public void InitToken();
        /* Clean up method to call when the connector becomes useless */
        public void End();
    }
}