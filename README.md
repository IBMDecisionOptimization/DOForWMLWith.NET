# Decision Optimization (DO) on Watson Machine Learning (WML) from .NET

This repository includes code to support the execution of IBM Decision Optimization (DO) on Watson Machine Learning (WML) from .NET (CPLEX or CPO) models.

DO is a set of modeling and solving libraries, along with development and deployment tools for prescriptive analytics using Mathematical and Constraint Programming techniques.

WML offers a Python and REST API to develop and execute DO models.

The code included in this repository provides two ways to use WML to solve DO models.
1. the `ILOG.CPLEX.*` and `ILOG.CP.*` packages include classes to easily modify existing .NET code so that the optimization will execute on WML.
The .NET code should include both the modeling and execution of the DO model, and **after just a single line change, execution will occur on WML**.
2. the `COM.IBM.ML.ILOG.*` package includes lower level classes to create **models** and **deployments** in WML, and then create and poll **jobs**.
With these classes, you can have detailed control on the execution and integration of DO in WML.

* The 1st packages are useful if you use CPLEX/CPO engines from .NET and want to delegate the solve resources to Cloud.
* The 2nd package is relevant if you want to run Python or OPL based jobs on the Cloud.

You have to build this library yourself with Visual Studio.

This library is delivered under the Apache License Version 2.0, January 2004 (see LICENSE.txt).


## Pre-requisites
This section describes some common requirements for both packages.

### .NET
Of course, these packages are C# code, and can only be used with your .NET developments.
The requirements for .NET versions are the same as for CPLEX.
This package runs with any edition of the CPLEX Optimization Studio 12.10 and 20.1 libraries on the client side (where your code is executed).
CPLEX/CPO supported version on WML side, where the solve is delegated, may depend on your Cloud Pak for Data version. Currently, 12.9 and 12.10 are supported.

### CPLEX installation
You will need a CPLEX installation in order to compile and run this code.

You don't need an official commercial version of CPLEX to run this as the optimization will happen on WML.

The compilation and modeling can be done with the **Community Edition** of CPLEX that is freely available.

### WML instance and credentials
In order to run on WML, you will need an instance with some credentials.

These packages are compatible with all versions of Cloud Pak for Data (Public and Private Cloud) Watson Machine Learning v4.

You can refer to the [DO for WML documentation for Public Cloud](https://dataplatform.cloud.ibm.com/docs/content/DO/DODS_Introduction/deployintro.html?audience=wdp&context=cpdaas) or [DO for WML documentation for Private Cloud](https://www.ibm.com/support/knowledgecenter/SSQNUZ_3.5.0/do/DODS_Introduction/deployintro.html) or to [this post](https://medium.com/@AlainChabrier/use-do-on-different-wml-locations-31e353955088).


### Project settings
This repository is built around a Visual Studio 2019 project which you can quickly reuse.
But you should be able to adapt to any visual version environment that you prefer.

As always the important things to check are:
 * the dependencies on the Concert and CPLEX .NET packages,
 * and the right setting of the path to the CPLEX native library.

 Before opening DOForWMLWith.NET, you need to set the **CPLEX_Studio_HOME** environment variable to point to a valid CPLEX Studio installation.

## The `ILOG.CPLEX` and `ILOG.CP` packages

Using these packages, you can very easily adapt an existing DO .NET code, with modeling and execution parts, so that the execution will occur on WML instead of being run locally.
These classes reuse the `COM.IBM.ML.ILOG` package but hide the complexity and the detailed control.
From outside of the engine class, everything will behave the same (see supported API and limitations) as if the solve was happening locally.

### How to start?

You first need to be familiar with the Watson Machine Learning environment and the assumption of this library is that you can provide a valid **Space id** aka **deployment Space** alongside other ids.

You can start looking at one of the examples.

You can see in the examples, that from an original code where the engine is created using:

```
Cplex cplex = new Cplex();
```

In order to solve on WML, you just need to use the new class and provide some WML credentials (see the section below about creating your own [credentials class](#WML-credentials)).

```
Cplex cplex = new WmlCplex(credentials, runtime, size, numNodes);
```
where:
* **Credentials** credentials: contains the list of your credentials. You can easily create one via code or from a configuration file.
you can specify your WML credentials, either by hard-coding them in those files or by overloading the environment variables.
* **Connector.Runtime** runtime: version of the runtime (DO in WML, where the solve is delegated, supports the last 2 versions of CPLEX Studio, currently 12.9 and 12.10)
* **Connector.TShirtSize** size: size of the WML job
* int numNodes: number of nodes.

The new CPLEX class will behave exactly like the original one (see [supported API and limitations](#Supported-CPLEX-and-CPO-API-and-limitations)), you can call the solve, get the variables values, an so on.

The same applies to the IloCP class where
```
CP cp = new CP();
```
is replaced with
```
CP cp = new WmlCP(credentials, runtime, size, numNodes);
```

For example,
```
Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);

```
will create:
   * a CPLEX engine for WML
   * getting the credentials from the current configuration file
   * For a 12.10 version of DO
   * with a medium size
   * and 1 node

### Supported CPLEX and CPO API and limitations

With both CPLEX and CPO, you can build any type of model as long as you don't use any callbacks, set parameters and delegate the solve to Cloud Pak for Data.

Both engines have unsupported methods.
When such an unsupported method is called, an exception will be raised in most cases at execution time.

#### CPLEX Supported/Unsupported API

You can build, solve or relax (partial support), query variable values, objectives and also get slacks/duals/reduced costs.

In few words, solving and getting values/reduce costs / slacks / duals  is supported.
Other methods are not supported, for example related to:
* Callbacks/Goals
* Solution pools
* Asynchronous api
* Conflicts
* Tuning methods

See [`Details about CPLEX`](CPLEX.md) for a detailed list of methods.

#### CP Supported/Unsupported API

All model building methods are supported.

Solve and refine conflict methods are executed remotely.
Name constraints with explicit labels when invoking conflict refiner as returned conflicts will use these labels to identify conflicting subset of constraints.
You can set parameters.
You can query values for variables, interval variables, and get objective(s) value(s). Methods where a name is passed are supported, while methods where the object is passed are generally not supported for scheduling objects.


* All methods related to callbacks (non-batch solving) are not supported.
* A number of additional methods like failure explanation or search information are not supported.
* Others methods are not supported but a work-around can be used (for instance: `getValue(IloIntExpr expr)` can be replaced with: `getIntValue(IloIntVar idvar)` after adding an equality constraint binding the integer decision variable to the expression value).

Invoking any of the non-supported methods may result in an exception, non controlled behavior or crash.

See [`Details about CPO`](CPO.md) for a detailed list of methods.


## The `COM.IBM.ML.ILOG` package

This package contains the classes you need to use to pilot a fine grained handling of WML job.
They might not be useful if you only use CPLEX/CPO jobs, but are relevant if you want to use Python-based jobs, or OPL-based jobs.

* If you are interested in the WML api, see the [`BrowseWML.cs`](Tests/WML/BrowseWML/BrowseWML.cs) example.
* If you are interested in delegating a Python/OPL based scenario, see the [`WMLSamples.cs`](Tests/WML/WMLSamples/WMLSamples.cs) example.

Library execution can be controlled by config files, environment variables, where you can specify with hard coded values or environment variables various parameters such as:
* the default time limit (If no time limit is provided by the engines, then by default, the WML job will be terminated after this time limit to avoid wasting resources).
* the frequency of status checks (by default 500msec between 2 checks).
* the engine log level on the WML side.
* if the library must write all artifacts and WML payloads/answers for job input/output to the disk (useful for debugging) in an existing directory.
   * the version of the model
   * parameters, filters, mip starts...
   * WML payload for job submission endpoint
   * WML answer when the job is finished.  

You can also control the log verbosity of the library using log4j configuration.

The main classes are:
* [`Connector`](Source/WMLBase/COM.IBM.ML.ILOG/Connector.cs) which provides an interface to handle the WML endpoints.
* [`COSConnector`](Source/WMLBase/COM.IBM.ML.ILOG/COSConnector.cs) which provides an interface to help handle the IBM Cloud Object Storage that is used in Cloud Pak for Data public.
* [`Job`](Source/WMLBase/COM.IBM.ML.ILOG/Job.cs) which provides an interface to handle a WML job and its inputs/outputs.
* [`Credentials`](Source/WMLBase/COM.IBM.ML.ILOG/Credentials.cs) which provides a helper to handle WML and the platform credentials.

## WML credentials

All your credentials to use WML are provided to the library through the [`COM.IBM.ML.ILOG.Credentials`](Source/WMLBase/COM.IBM.ML.ILOG/Credentials.cs) file.

You can either complete this object:
* by providing a Configuration file.
* or by providing the keys/values yourself.

For public Cloud, the following entries must be provided:
   * service.iam.host
   * service.iam.url
   * service.wml.host
   * service.wml.api_key
   * service.wml.space_id
   * service.wml.version
   * service.platform.host

For private Cloud, the following entries must be provided:
   * cpd.url
   * cpd.username
   * cpd.password
   * cpd.wml.host
   * cpd.wml.space_id
   * cpd.wml.version

## Note about the library and WML interactions.

Running a DO job in WML needs a WML Deployment job.
The creation of a job takes approximately 20 seconds, so to avoid this delay, the library reuses deployments when a new job is triggered.
Deployments can be created if not present: they will be created/reused with a name that is the combination of
   * the engine
   * the runtime
   * the size
   * the number of node
An example of a deployment name is `CPLEXWithWML.do_12.10.M.1`

As such, the first time you use an empty Deployment Space, you will get this 20 second delay, which will disappear at next call to CPLEX/CPO.
If you want to avoid this, you can create these deployments yourself before any use of CPLEX/CPO: see the [`PrepareWML.cs`](Tests/WML/PrepareWML/PrepareWML.cs) sample to help you in this task.

## How to run the examples.

The examples are using configuration files and environment variables to handle the various endpoints/credentials you need to provide to make them run.
you need to either hardcode them in the config files or define the env vars.

Here is a list of the variables you need to setup to run them.
To run the CP/CPLEX examples, you only need to provide the variables related to Watson Machine Learning.
To run the WML workflow examples, you will need to provide the variablesrelated to Cloud Object Storage also

**Watson Machine Learning related**
   * WML_HOST: defines the host, for example  "https://us-south.ml.cloud.ibm.com".
   * WML_API_KEY: your api key.
   * WML_SPACE_ID: defines the space that will used to store/handle the various artefacts (model, deployment, ...).


**Cloud Object Storage related**
   * COS_ENDPOINT: defines the COS endpoint, for example "https://control.cloud-object-storage.cloud.ibm.com/v2/endpoints".
   * COS_BUCKET: your bucket name.
   * COS_ACCESS_KEY: access key.
   * COS_SECRET_KEY: secret access key.
   * COS_ORIGIN_COUNTRY: from the configuration of the bucket, in Bucket details, pick the location, for example "eu-de" if you find "Location eu-de".

## Library dependencies.

This library is based on open-source libraries.

Here is the list of these:
* https://logging.apache.org/log4net/
* https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md

Right now, this library only supports the [**ml/v4**](https://cloud.ibm.com/apidocs/machine-learning) current version.

It only uses the public WML/DO/CPLEX api.

## Get your IBM® ILOG CPLEX Optimization Studio edition
You can get a free [`Community Edition of CPLEX Optimization Studio`](https://www.ibm.com/account/reg/us-en/signup?formid=urx-20028), with limited solving capabilities in terms of problem size.

Faculty members, research professionals at accredited institutions can get access to an unlimited version of CPLEX through the [`IBM® Academic Initiative`](https://community.ibm.com/community/user/datascience/blogs/xavier-nodet1/2020/07/09/cplex-free-for-students?CommunityKey=ab7de0fd-6f43-47a9-8261-33578a231bb7&tab=).

## Support

This open source contribution is supported by the Decision Optimization team.
You can reach us directly or via the standard Support channels.

If you want to submit a bug, please send us among the details:
   * The OS of your machine.
   * Your .NET version.
   * Your CPLEX Studio version.
   * The Version of this library.
   * The space id, deployment id and the job id from your logs.

We might also ask you to send, if you agree:
   * the logs produced by this library.
   * a zip containing the output of your run when `export_path` is enabled in the resource configuration, in order to reproduce the problem.


## Other reading
Read more about DO and WML by following [Alain Chabrier](https://medium.com/@AlainChabrier).
