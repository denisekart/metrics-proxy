# Solution design - a draft

The application will be a proxy between a series of metrics data sources and a series of data sinks.

A data source will be a service that will operate in a "pull" mode. Data will be queried periodically from a data source. 

A data sink will be a service that will operate in a "push" mode. Data will be pushed periodically to a data sink.

The system will have a clock. An event on the clock will trigger a "pull" and subsequently a "push" of the data.

The system will keep a log of operations that were performed. The data queried will be stored in the system. The data sent will be logged. Status of the sent data will be tracked (success, failure).

A periodic trigger will query all of the registered data sources for new data. Any received data will be stored locally. A periodic trigger (can be the same trigger) will send the data not yet sent, to any and all data sinks.

The application will expose an API endpoint for querying the current data status. When a user queries the endpoint, a JSON payload will be returned. The payload will contain the requested statistical data (to be defined later).



## Data sources

A data source will be defined as a single interface (an API). External actors will create implementations of said interface. The interface will  be "registered" in the application pipeline. The data source will expose a property `name` and an operation `query` . 

`name` will specify a unique identifier of a data source. 

`query` will trigger an operation which will result in a map of key-value pairs. a `key` will represent the KPI identifier. A `value` will represent a value for that KPI.



## Data sinks

A data sink will be defined as a single interface. External actors will define implementations of the interface. The interface will be registered in the application pipeline. 

The sink will expose a property `name` which will uniquely identify the sink. 

A method `report` will commit the data that is provided in the input to the data sink.

> I'll make sure that the KPIs being reported will be globally unique (e.g. prefix kpi name with the data source name).



## The API endpoint



## The client side

