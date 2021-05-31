using System;
using Newtonsoft.Json.Linq;

namespace COM.IBM.ML.ILOG
{
    public enum Runtime
    {
        DO_12_9,
        DO_12_10,
        DO_20_1
    };

    public enum ModelType
    {
        CPLEX_12_9,
        CPO_12_9,
        OPL_12_9,
        DOCPLEX_12_9,
        CPLEX_12_10,
        CPO_12_10,
        OPL_12_10,
        DOCPLEX_12_10,
        CPLEX_20_1,
        CPO_20_1,
        OPL_20_1,
        DOCPLEX_20_1
    };

    public enum TShirtSize
    {
        S,
        M,
        XL
    };

    public interface Connector : TokenHandler
    {
        String GetOrMakeDeployment(String name, bool isCplex);

        public String CreateNewModel(String modelName, Runtime runtime, ModelType type, String modelAssetFilePath);
        public String DeployModel(String deployName, String model_id, TShirtSize size, int nodes);

        public JObject CreateDataFromString(String id, String text);
        public JObject CreateDataFromFile(String id, String fileName);

        public Job CreateJob(String deployment_id,
                            JArray input_data,
                            JArray input_data_references,
                            JArray output_data,
                            JArray output_data_references);
        public Job CreateAndRunJob(String deployment_id,
                                  JArray input_data,
                                  JArray input_data_references,
                                  JArray output_data,
                                  JArray output_data_references);

        public JObject GetDeployments();
        public JObject GetModels();
        public JObject GetDeploymentJobs();

        public String GetDeploymentIdByName(String deployment_name);

        public void DeleteModel(String id);
        public void DeleteDeployment(String id);
        public void DeleteJob(String id);
        public int DeleteModels();
        public int DeleteDeployments();
        public int DeleteJobs();
        public void CleanSpace();
        //public void InitToken();

        public JObject GetInstances();

        public JObject GetStorage();

        public String GetCatalogId();

        public JObject GetSoftwareSpecifications();

        public String CreateDeploymentSpace(String name, String cos_crn, String compute_name);

        public JObject GetDeploymentSpaces();

        public String GetDeploymentSpaceIdByName(String spaceName);

        public JObject GetAssetFiles(String space_id);

        public JObject GetStorageBySpaceId(String space_id);

        public String GetCatalogIdBySpaceId(String space_id);
    }
}
