# .Net Core Collector SDK
A .Net Core SDK for Ivanti Connectors that allows the loading, transforming and publishing of data.  It is a configuration driven ETL tool for building Ivanti Connectors.

# Introduction
The purpose of this document describes the design and implementation of the .Net Core Collector SDK.

# Terminology
Reader:  The component that allows data to be collected from a system.  For example, in Discovery Services, a “connector” to EPM that provides a Web API so that it can get its database and SCN data.

Transformer: Transforms the data using some type of mapping configuration into a format that Discovery Services can consume.

Mapper: Maps data row by row performing both left and right transformation using a mapping definition.

Converter: Converts a data point from one format to another.

Publisher:  Publishes transformed data to Uno,  Default is via the Agent Frameworks communication channel to Uno and the Agent Base Layer Services.

End Point: The location of either a data source or a data repository.

Collector:  A container for the Readers, Transformers and Publishers.  Its main responsibility is to dynamically create and execute the collector micro-system.

Stack: Contains a single stack consisting of a single Reader and the Transformers/Publishers associated with that Reader.  It is responsible for executing and coordinating all the layers.

# Overall Architecture
The Collector SDK is designed with a layered architectural style in mind.  Readers (on the bottom) querying a data source, bubble that data up to Transformers to transform the data and finally Publishers to publish the data to some repository.  Configuration is used to define not only configuration parameters (such as login credentials) but to also define the classes to create and use at each layer.
 
# Collectors     
Collectors can run either on premise or in the cloud.  
Collectors receive and monitor state events from Readers, Transformers and Publishers.  
Multiple Collectors can be scheduled and run in parallel.  
Configuration is generic and can be done via the Agent policies or via app configuration.
 

# Stacks     
Stacks are now responsible for executing and coordinating a stack.  
Each Stack is created from the configuration and is stand alone.  This ensures data integrity in a multiple reader environment.  
The Stack is registered as the Data Handler and Publish Data callbacks, running the configured Transformers and Publishers when signaled.
 

# Readers
Readers are responsible for connecting to a data source, querying data based on a set of rules and handing off that data to Transformers.  
Readers are dynamic and configurable at run time.  
Readers are generic, they know how to speak SQL, REST, CSV, XML, etc.   
Data is read in pages and is configurable.
 

# Transformers   
Transformers convert data into a format that the target service (data repository) understands.  
Transformers are dynamic and configurable at run time.  
Transformers are configured to use Publishers to publish the data.  
Transformers are configured to use Mappers to do most of the heavy lifting of converting data.
 

# Mappers     
Mappers map incoming data into a usable outgoing format using converters.  
Mappers are configured to use a mapping definition that defines the incoming data by key (typically column name) and uses dynamic converters to perform right/left transformation.  
Mappers are dynamic and configurable at run time.
 

# Converters     
Converters convert columns of data referred to as data points.  
Data points are name/value pairs where the value can be any object.  
Converters are associated with the incoming data by key, i.e. column name.  
Converters are dynamic and configurable at run-time.
 

# Publishers     
Publishers publish the data and are similar to connectors, they know how to communicate to a service end point where the results will be stored.  
Publishers are dynamic and configurable at run time.
Collector SDK Interfaces

# Piped Converters
Any Extract Load Transform (ETL) system allows you to pipe data from one transformation to another.  This is complex and was not required for version 1.x.  Piped Converters is the first step towards this concept.  Piped converters allow you to:

Associate multiple converters with a single data point key (now called a Primary Key).
Targeted converters to have their own left side mapping at run time.
Data from one converter is piped to other configured converters.  The piping of the data has two concepts.
Data is saved (the data returned from the mapping operation) when the last converter is invoked.
Data is saved at each conversion point.  Each converter gets the original data point plus all other previously converted data points. As each converter is invoked, any converted data is saved (i.e. this data will be eventually be published).

The Collector contains 1 to many stacks, this is the same number of Readers that is desired to run independently.  This is up to the Collector implementation to decide.  For example it may choose to create a new Stack for each new data source, such as a file.  Or it may simply create a Stack for each configured Reader.  Once the Stack is created it can be fired off to execute,  The Stack receives all Data events and passes those events off to the appropriate Transformers or Publishers.  The Collector receives state events signaled by the different layers.  At a minimum each layer should signal that it has completed its work, so that the Collector can monitor progress of the layers.  The following outlines the responsibilities of each component within the system.

# Several interfaces are defined in the SDK, they include:
     
EndPointConfiguration - Defines the connection parameters required to communicate with the data source.      
           
Id - The id of this configuration (this can be any string, but it must be unique).
User - The user name used to login to the source.  Can be null.
Password - The password used to login to the source. Can be null.
Properties - Name/value pairs defined as a set of IDataSourceProperty.
GetDataSourceProperty(string name) - Convenience method for accessing a property by name.
      
IReader- Defines the interface for query data from some data source.  It uses the EndPointConfiguration which is required to communicate with the data source.
            
Id - The id of this reader (must be unique).
Configure(string id, EndPointConfiguration config, IDataHandler handler) - Method to allow configuration of the Reader.
Run() - Runs the Reader.  Note: this is meant to be a single operation, i.e. connect, query, disconnect and hand-off data to a Data Handler.
       
ITransformedDataHandler - Defines a partial interface for handling data from a Reader (this was to limit dependencies on above layers).              
PublishData(string senderId, List<object> data, Dictionary<string, string> context) - Handle the data queried by a Reader.  A transformer would typically implement this interface, but it is not required.  The context allows lower layers to pass along contextual information about the job.
        
IEntityCollection - Defines an interface for a dictionary of data points (entities).      
           
Entities- A dictionary of data points.
        
IEntity - Defines an interface for a data entity.  Currently it is only a place holder, however any objects that are created by converters and mappers must implement this interface.
   
ITransformer - Defines the interface for a Transformer.      
           
Id - The id of this transformer (must be unique).
Configure(TransformerConfiguration config, ITransformedDataHandler handler) - Method to allow configuration of the Transformer.
HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context) - Transforms the data using Mappers/Converters and invokes transformed data handler. The context allows lower layers to pass along contextual information about the job.
   
IMapper - Defines an interface for a Mapper.  The Mapper uses a set of Converters to convert data points.      
           
Id - Id of this Mapper (must be unique).
TransformerId - Id of the associated Transformer.
Configure(string Id, string dataType, string transformerId, Dictionary<string, IConverter> converters) - Method to allow configuration of the Mapper.  Takes a dictionary of Converters, to be used to convert data points.
List<object> Map(List<IEntityCollection> data) - Convert data into something else.  For example you may merge data points, delete them, rename them, convert them, etc.  Most work would typically be done by the set of converters, but is not limited to that.
     
IConverter - Defines an interface for performing left and right transformation.      
           
InputName - Identifies the name of the data point to convert.
OutputName - The left side transformation of this data point, i.e. InputName -> OutputName.
Configure(string inputName, string outputName) - Method to allow the configuration of the Converter.
IDataPoint Convert(IDataPoint point, IData row) - Convert the data point.  The entire row is provided just in case there are other column dependencies.  Return null to delete the property.
        
IPublisher - Defines an interface for a Publisher.      
           
Id - The id of this publisher (must be unique).
Configure(string id, EndPointConfiguration config) - Method to allow the configuration of the Publisher.  The config contains the parameters need to connect to a data repository.
PublishData(string senderId, List<object> data, Dictionary<string, string> context) - Publish the data to a data repository.  For example this could post data to the Discovery Services Rest API.�� The context allows lower layers to pass along contextual information about the job.


# Configuration Changes in 2.0
The following changes have been made to support piped converters.

Collector Version - There is now a version within the Collector configuration as a double.  Default is 1.0 (backward support).  Version 2.0 + indicates piped converter support.

Mappers.PipedConverters - These are all the configured converters, this overrides Mapped.Converters.

Id - The id of the converter. (required)

Type - Class type of the converter. (required)

Mappers.SourceTargetMappings - Defined the mappings between the inbound data and the piped converters.

PrimaryKey - The data point key.

Properties - A dictionary of name/value pairs.

TargetConverters - A list of converters to invoke (in order) for this data point.

Id - Id of the converter.

CombineInputOutput- Set to true if both the input to a converter and the output should be combined

Nested - Set to true if the output should be run through all the mappings again.  This allows you to nest conversions of a hierarchy. 

LeftSideMap - Rules when converting the primary key to another key.
 

# Example Configuration
In the example below the Mapper is configured for two Converters: CombineDataTimeConverter and DateTimeUtcConverter.  Note the lack of the LeftSideMap.  This is now injected into the Converter at run time, not configuration time.  It is now defined in the SourceTargetMappings.  In the SourceTargetMappings example we are not saving the converted data until the end (PipedData = true).  The PrimaryKey is the "Date" field within the row of data signaled by the Reader.  That data point is first fed into the Converter that expects a "Time" field in the data row and combines those into a date time.  The combined date time is then fed into the UTC Converter.  The output that gets saved (and eventually published) is the combined date time converted to UTC.  See these tests to get an idea of how to use the piped converters.

...

"Mappers": [
   {
      "Id": "50",
      "TransformerId": "40",
      "Type": "Collector.SDK.Mappers.DefaultMapper,Collector.SDK",
      "PipedConverters": [
         {
            "Id": "1",
            "Type": "Collector.SDK.Converters.CombineDateTimeConverter,Collector.SDK",
            "Properties": {}
         },
         {
            "Id": "2",
            "Type": "Collector.SDK.Converters.DateTimeUtcConverter,Collector.SDK",
            "Properties": {}
         }
      ],
      "SourceTargetMappings": [
         {
            "PrimaryKey": "Date",
            "TargetConverters": [
               {
                  "Id": "1",
                  "CombineInputOutput": "false".
                  "NestOutput": "true",
                  "LeftSideMap": { "Date": [ "DateTime" ] }
               },
               {
                  "Id": "2",
                  "CombineInputOutput": "true".
                  "NestOutput": "false",
                  "LeftSideMap": { "DateTime": [ "DateTimeUTC" ] }
               },
               "Properties": {}
            ]
         }
      ]
   }
]

 

# Mapping/Piping Concepts
A couple of new Mapping concepts have been introduced: Piping of data and nested conversions. 

The piping of data is currently based on two rules:

The mapper does not attempt to save the input, it only cares about the output.  In this case it is up to the converters to decide what to output.
The mapper will combine the input with the output.  This is for the case where you want to create an additional property. For example converting a local date time to UTC would result in the local date time as well as the local date time in UTC (example configuration above).
The nesting of converters allows you to nest the output.  This is an implicit piping versus explicit (contained within one source target map).  If enable on a converter, the mapper will take the output and run it through all source target mappings, not just the containing map.

See these tests to get a better idea of how these new mappers can be used.

# About the Tests

Test: PipedConverters_PipedArray_Success
In this test a data point contains an array of objects (software items).  This is testing the PipedArrayConverter and its ability to do a nested conversion an array of objects.  The result is a dictionary of dictionaries based on an id. The id is determined in the SourceTargetConverter property "ArrayKey".  The property "PrefixId" means to prefix the id with the "ArrayKey", i.e. "Id_XXX" where XXX is the Id field of the software.. The PipedArrayConverter expects the data point to be a list of objects.  It then iterates through the list and uses reflection to access the getters for each field in that object.  For each field it attempts to look up a converter by name.  The name must be contained in the left side mapping of the converter.  If a converter is found it converts the fields values and inserts the result into the output. It can can do all this because the SourceTargetMapping and the Converters in that mapping are now accessible by the Converter. Note: In a real implementation this is dependent on the mapper, the mapper must create the converter list and configure each of them properly at run time, see the AbstractMapper.ConvertDataPoint() method.

Test: PipedConverters_ConvertObjectArray_Success
This tests the same concepts as above except that it uses a configuration file instead of programmatic configuration.  This not only tests the converter but the mapper as well.

Test: PipedConverters_ConvertDelimitedDataPoint_FileConfig_Success
This tests nesting and piping of data.  A converter creates multiple data points from a single data point that is a delimited array of values.  The DelimitedConverter takes as a property an array of delimiters.  It uses the array to split the data point's string value into an array of values.  The values key is computed by the LeftSideMap which contains a column index as the key.  In this example the configuration for the converter looks like:

{

"PrimaryKey": "Items",
"Properties": {
   "ArrayDelimeters": ","
},
"TargetConverters": [
   {
      "Id": "3",
      "NestOutput": "true",
      "CombineInputOutput": "false",
      "LeftSideMap": {
            "0": [ "Name" ],
            "1": [ "Time" ],
            "2": [ "Date" ],
            "3": [ "SerialNumber" ],
         }
      }
] }

This means the first four values of a delimited string of "Some Software Title,01/17/2018,01:23:03.000,00000-789-1234567890" will convert to { "Name":"Some Software Title", "Time":"01:23:03.000", Date": "01/17/2018", "SerialNumber": "00000-789-1234567890" }.  Since this is a converter that has its NestOutput set to true, the mapper then runs the output of the DelimitedConverter against the rest of the configured converters.  Running all of the output data points against any other configured converters.  In this example there are the following converters:
{
   "PrimaryKey": "Date",
   "TargetConverters": [
   {
      "Id": "1",
      "NestOutput": "true",
      "CombineInputOutput": "false",
      "LeftSideMap": {
         "Date": [ "DateTime" ]
      }
   } ]

},
{
   "PrimaryKey": "DateTime",
   "TargetConverters": [
   {
      "Id": "2",
      "NestOutput": "false",
      "CombineInputOutput": "true",
      "LeftSideMap": {
         "DateTime": [ "DateTimeUTC" ]
      }
   } ]
},
{
   "PrimaryKey": "Name",
   "TargetConverters": [

   {
         "Id": "4",
         "NestOutput": "false",
         "CombineInputOutput": "false",
         "LeftSideMap": {
            "Name": [ "Title" ]
         }
   } ]
},
{
   "PrimaryKey": "SerialNumber",
   "TargetConverters": [
   {
      "Id": "4",
      "NestOutput": "false",
      "CombineInputOutput": "false",
      "LeftSideMap": {
      "SerialNumber": [ "Id" ]
      }
   } ]
}

In this example "Date" pipes into CombineDateTimeConverter, which pipes into DateTimeUtcConverter.  The output of this conversion is 2 data points: "DateTime" and "DateTimeUtc".  This is because the CombineInputOutput is set to true in the DateTimeUtcConverter, which means the input "DateTime" is combined with the output "DateTimeUtc"  The "Name" and "SerialNumber" are both piped to the NoOpConverter. These are all returned as a dictionary that is then converted into a MockSoftware object (the mapper DataType is set to "Collector.SDK.Tests.Mocks.MockTransformer,Collector.SDK.Tests").
