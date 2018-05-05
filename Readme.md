i# Premonition Placement

This code demonstrates calling the AI for Earth land cover API, in a use case involving Project Premonition. 

This demo makes use of the [AI for Earth Landcover](https://www.microsoft.com/en-us/aiforearth/land-cover-mapping.aspx) API as well as the [Azure Maps platform](https://aka.ms/AzureMaps). 

## Setup

1. Sign up for an AI for Earth subscription key on Azure.
2. Sign up for an Azure Maps subscript key ([details](https://docs.microsoft.com/en-us/azure/location-based-services/how-to-manage-account-keys))
3. Add these subscription keys into the appropriate appSetting in the App.config file of the project. 
4. This demo does not work with the Any CPU" setting in Visual Studio. If you have issues running this demo change the platform target to x86 or x64 by right clicking on the solution, then select "Configuration Manager", and set the "Active soultion platform" to x86 or x64.

![AI for Earth Landcover Map](AiForEarthLandcoverMap.png)
