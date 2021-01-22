using System;
using Newtonsoft.Json.Linq;


namespace COM.IBM.ML.ILOG
{
    public interface COSConnector : TokenHandler
    {
        /*
        Builds a data reference from an id.
         */
        public JObject GetDataReferences(String id);

        /*
        Method to upload a file on the disk to a COS bucket.
         */
        public String PutFile(String fileName, String filePath);

        /*
        Download the content of a COS file as a string.
         */
        public String GetFile(String fileName);
    }
}
