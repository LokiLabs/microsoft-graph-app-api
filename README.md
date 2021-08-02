# Prerequisites
* Visual Studio

# How to run this repo locally?
1. Open windows terminal
1. Git clone this repository
1. Open file explorer
1. Navigate to the path that you cloned the repository to
1. Double click on `MSGraphWebApi.sln`. Make sure that this opens in Visual Studio
1. Open Solution Explorer (View > Solution Explorer)
1. Navigate to `GraphWebApi` > `appsettings.json` in Solution Explorer and open up the json by double clicking
1. Fill in the tenantId, clientId, clientSecret and AzureConnectionString for blob storage with the appropriate information
1. Save the file
1. Right click on the `GraphWebApi` folder in Solution Explorer
1. Click on `Build`
1. Run `IIS Express` for `GraphWebApi`
![Screenshot 2021-08-02 025604](https://user-images.githubusercontent.com/47228306/127843924-b5cc1849-833d-47e3-b795-7b328b6ad1e2.png)
1. Open Postman and run queries (eg. https://localhost:44399/graphproxy/v1.0/)

# How to deploy?
1. Right click on the `GraphWebApi` folder in Solution Explorer
2. Click on `Publish`
3. Follow the prompts and select how you want to deploy
