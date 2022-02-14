# CI/CD solution with Github, Azure Devops, Azure Cloud and CLI/PowerShell - So simple!

# Overview - What problem are we solving?
[show a block diagram of CI/CD, SOURCE CODE --> BUILD --> DEV-->UAT-->PROD]
[talk about CI/CD challenge, taking a product from code to deploymnent, passing via build, unit tests, dev, uat and prod stage]

#### Importance of multiple infrastructure environments
to be done
- Significance of Dev stage
- Significance of Uat stage
- Significance of Prod stage

#### Automate the deployment of infrastructure
Talk about IAC and 


#### Improving the Pull Request experience?
Talk about when a developer raises a Pull Request. Automated CI kicks in --> Automated CD kicks in


---

# Why did I write this article?

I wanted to demonstrate that if we stick to the fundamental tools like Azure CLI, Powershell, Github and Azure Devops - then a fairly complex can be solved with relative ease. I often hear developers stressing how important it is to use a 3rd party Cloud management product like Terraform or Pulumi. If you are on Azure, then CLI and PowerShell are all you need for your "Infrastructure as code" solution.

---

# What sort of application are we automating in this article?

[ show a picture of the blocks , web app, static site]

- out of box web app which generated dummy weather forecast
- simple static HTML which invokes the REST end point and renders the forecast


---

# What are the key technologies involved?

- Github for source code
- Azure Devops for build (CI) and release(CD) pipelines
- 
---

# What is the desired development workflow?
[show a flow chart]
code checkin --> PR raied --> CI build pipeline kicks in --> build and tests pass --> CD staqe kicks in --> deployment to Azure --> Developer gets to verify if checked in code is producing the desired functional resutls (e.g. UI tests)

---

# How do we structure the Github repo?

1. Talk about mono repo vs multiple repo. 
1. Show a tree diagram of the top level. 
1. Explain the files  (YML, Powershell)

---

# How do we structure our assets on the Azure cloud?

1. Show a pic of resource groups
2. Explain the naming of the resource groups

# How does Azure Devops help us orchestrate our CI/CD pipelines?

az devops will listen for checkin events via web hooks and kick of the build pipelines

![Devops-Github](docs/images/ms-azdevops-github.png "devops github")

show a picture , similar to above, with some arrows, depicting code checkins, split into CI+CD

---

# What is the responsibility of the Continuous Integration(CI) pipeline?

Talk about the basic steps in a CI pipeline, code checkout, compilation, unit tests, code coverage

Give an example of YML  with build, unit test and publish steps


---

# What is the responsibility of the Continuous Integration(CD) pipeline?

Talk about , taking the drops from CI and deploying on DEV. Create DEV infrastructure, deploying on DEV, followed by UAT and PROD

---

# How to create a CI pipeline in Azure Devops and link with code in Github ?

Show the visual steps here

# How does Azure Devops automatically determine which CI pipeline to run ?

Explain the problem of a mono-repo. The YML specification addresses this problem using the elements `trigger` and `pr`

#### Example snippet
Every YML should have something similar at the very beginning of the file

```
trigger: 
  branches:
    include:
    - master 
  paths:
    include: 
    - '/DemoWebAppWithCiCd/BackEnd/*'

pr: 
  paths: 
    include: 
    - '/DemoWebAppWithCiCd/BackEnd/*'

```
#### Why do we need a pr element?
to be done
#### Why do we need a trigger element?
to be done

---

# How to create a Continuous Delivery (CD) release in Azure Devops?

to be done. Show the steps

---

# How does the CD stage get triggered by the CI pipeline?

to be done.
Show the trigger and drop config

---

# What happens when a Pull Request is raised?

- Take the example of a simple back end change
- Show pic of creating PR
- Show status checks in Github
- Show CI stage getting kicked in
- Show CI completing and CD starting
- Show status checks in Github

---

# How to create the assets on Azure cloud using PowerShell and CLI?

Do not get into too much details. 
Talk about a PS script that can be used locally
Stress on not using Devops for debugging but only for automation

---

# How does the CD stage kick off the PowerShell/CLI scripts?

Show a pic of Devops Task with Powershell configured
Show the pic of options

---

# How to test the infrastructure code by running it on your workstation?

Talk about `set-azcontext` and then running the scripts
1. BackEnd/infrastructure/createwebapp.ps1
1. BackEnd/infrastructure/deploy.ps1


# Getting started with the accompanying code

#### Link to accompanying source code
Link to Github

#### Building and deploying the BackEnd from your local workstation
to be done

#### Building and deploying the FrontEnd on your local workstation
to be done

#### Creating a CI pipeline
Refer steps in the section [TO BE DONE]

#### Creating a CD pipeline
Refer steps in the section [TO BE DONE]

#### Setting the Github branch policy
to be done, screen capture of Github which forces a PR

# References
https://mydemowebapi123.azurewebsites.net/WeatherForecast

---

# References
#### Azure Devops - Configure run or build numbers in Azure Devops
https://docs.microsoft.com/en-us/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml

#### Azure Devops - Use predefined variables
https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml#pipeline-variables-devops-services

#### Azure Devops - Add & use variable groups
https://docs.microsoft.com/en-us/azure/devops/pipelines/library/variable-groups?tabs=classic&%3Bview=azure-devops&view=azure-devops#use-a-variable-group

#### Azure Devops - Connect to Microsoft Azure
https://docs.microsoft.com/en-us/azure/devops/pipelines/library/connect-to-azure?view=azure-devops

#### Azure Devops - Combining Az Cli and Azure Powershell Az modules in a DevOps Pipeline
https://www.integration-playbook.io/docs/combining-az-cli-and-azure-powershell-az-modules-in-a-pipeline

#### How to deploy a Web App?
https://docs.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest#az-webapp-deploy

#### Create a CI/CD pipeline for GitHub repo using Azure DevOps Starter
https://docs.microsoft.com/en-us/azure/devops-project/azure-devops-project-github


#### How to build from command line?

The output path is missing
```
dotnet publish --configuration Release --output c:\truetemp\junk1234\ demo.sln
```
tobe done, how to set file version during build
