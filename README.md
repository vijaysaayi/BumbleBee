# Project-Penguin
Project Penguin aims to make **"Getting Started with Linux App Services"** simpler. <br>
It provides simple commands such as **create**, **deploy** and **use** to provide a playground for Azure Users.

## Prerequisite:
- An Active Azure Subscription

## Commands:
1. Create a new Python App Service:      
   > ```console
   > penguin create [-n/--name="Name of the App Service"] [-w/--with="Type of Dependent Resource"] [-r/--repo="Github Repo from which code should be deployed"] [-b/--branch="Name of Github repo branch"] 
   > ```
   > **Valid Arguments for --with = sqlserver , storage** <br/>
   >
   > Examples :
   > | Command  | Details |
   > | ------------- | ------------- |
   > | penguin create | Creates a Python 3.8 App Service  |
   > | penguin create --with sqlserver | Creates a Python 3.8 App Service with Azure Sql Server |
   > | penguin create --with storage -n "testapp" | Creates a Python 3.8 App Service named "testapp" with Azure Storage |
   > | penguin create --repo "https://github.com/Azure-Samples/python-docs-hello-world" | Creates a Python 3.8 App Service and deploys the code from specfied Github repository |
      
2. Deploy WebApp for Containers app using buildpack:      
   > ```console
   > penguin deploy [-n/--name="Name of the App Service"] [-b/--builder="Name of the builder"] [-r/--repo="Github Repo from which code should be deployed"] [-p/--port="Port number on which the App is listening"] 
   > ```
   > **Valid Arguments for --with = sqlserver , storage** <br/>
   >
   > Examples :
   > | Command  | Details |
   > | ------------- | ------------- |
   > | penguin deloy -r https://github.com/vijaysaayi/flask-example.git -b heroku/buildpacks:20 -p 8000 | Deploys an app with builde:heroku/buildpacks and specified source code  |
   
3. Add Dependent sources and add necessary connection strings / App Settings:      
   > ```console
   > penguin use [dependent resource type] [-n/--name="Name of the App Service"]
   > ```
   > **Accepted values for [dependent resource type] are sqlserver , storage** <br/>
   >
   > Examples :
   > | Command  | Details |
   > | ------------- | ------------- |
   > | penguin use sqlserver | Deploys Azure SQL Server  |
   > | penguin use storage -n "testapp" | Deploys Azure Storage and adds connection strings for "testapp" |
   
   
         


