# What this is project about?
to be done

- ~~Plan is created (DONE)~~
- ~~Create the app service (DONE)~~
- ~~Do the deployment manually (DONE)~~
- ~~Script the deployment of back end(DONE)~~
- ~~Create a folder with an index.html, index.js  (DONE)~~
- ~~Restructure FronEnd , create 2 subfolders 1)src 2)infrastructure  (DONE)~~
- ~~Rename **Demo** to **BackEnd** (DONE)~~
- ~~How to set configuration parameters (DONE)~~
- ~~Enable CORS (DONE)~~
- ~~a button which invokes the web api (DONE)~~
- ~~Front End - Create script to deploy html to Storage account (DONE)~~
- ~~Back End - Make the structure similar to Front End (infrastructure,src)~~
- Create new project in Azure Devops
- Create new service connection in Azure Devops (will need enterprise application)
- CI for Back end
- CD with 3 stages for Back end
- Change Web app naming so that it has an environment specific suffix for dev,uat and prod
- Configure PR policy for Back end
- CI for Front end
- Test PR cycle
- Change storage account so that it has an environment specific suffix for dev,uat and prod
- CD with 3 stages for Front end
- Configure PR policy for Back end
- Complete README.MD
- Complete Article for Medium


# Overview - What problem are we solving?
[talk about CI/CD challenge, taking a product from code to deploymnent, passing via build, unit tests, dev, uat and prod stage]
- Significance of Dev stage
- Significance of Uat stage
- Significance of Prod stage

# Solution block diagram
[ show a picture of the blocks , web app, static site]

# Developer flow
[show a flow chart]
code checkin --> PR raied --> CI build pipeline kicks in --> build and tests pass --> CD staqe kicks in --> deployment to Azure --> Developer gets to verify if checked in code is producing the desired functional resutls (e.g. UI tests)



# Challenges
## [warning]Unable to expand variable 'build.buildNumber'. A cyclical reference was detected.
https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml#pipeline-variables-devops-services
https://stackoverflow.com/questions/61895363/azure-devops-pipeline-cannot-expand-any-of-the-environment-variables-at-runtime


https://mydemowebapi123.azurewebsites.net/WeatherForecast


# How to deploy a Web App?
https://docs.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest#az-webapp-deploy


## How to build from command line?
```
dotnet publish --configuration Release --output c:\truetemp\junk1234\ demo.sln
```

# What to do in the front end?
- Create a new folder **FrontEnd**
- Do a `npm init`
- Set up a static web site (How?)
- You will need a GITIGNORE

# Front end
Link to README.MD of front end


# References
## Azure Devops - Configure run or build numbers in Azure Devops
https://docs.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml

## Azure Devops - Use predefined variables
https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml#pipeline-variables-devops-services

## Azure Devops - Add & use variable groups
https://docs.microsoft.com/en-us/azure/devops/pipelines/library/variable-groups?tabs=classic&%3Bview=azure-devops&view=azure-devops#use-a-variable-group

