# CI/CD solution with Github, Azure Devops, Azure Cloud and CLI/PowerShell - So simple!

# Overview - What problem are we solving?
Imagine a very simple distributed application comprising of the following:
1. Back end tier which implements a single REST end point
1. Front end comprises of a static HTML page which invokes the end point and renders the data. 
1. The code repository is maintained on **Github**
1. the build and deployment process is on **Azure Devops** and the actual application is deployed to **Azure App Service** and **Azure Storage Account**.

In this article, I have described in a step-by-step fashion how to implement a robust and repeatable **Continuous integration(CI)** and **Continuous deployment(CD)** process which takes the code commits and does the build, unit test, followed by deployment to various environments.  Links to accompanying code is towards the very end.

![devops flow](docs/images/ci-cd-flow.png "devops flow")


#### Why is it neccessary to have multiple environments (dev/uat/prod)?

- **Dev** - This is where the code that is comitted by the developer and not yet merged into the **master** branch is deployed. This environment gives the developer an opportunity to test her changes and get quick feedback. Pull Request comments are implemented and tested out.
- **Uat** - After the pull request has been completed, the code is now implemented to the **master** branch. This environment gives the developer and the product owners to examine the product and possibly recommend improvements.
- **Prod** - When the Product owner is happy , she would approve and trigger the deoployment of the **master** branch. This is a gated stage and might take into several factors. E.g. Deploy during quiter hours, do not deploy during long weekends, etc.

#### Why do we need to have a robust Pull Request experience?
You want the PR branch to be deoployed to a Dev environment. This is an environment that is safe and allows rapid feedback thereby leading to a better product quality
The PR deployment should be automated as much as possible.

#### How do you automate the deployment of infrastructure to the cloud?
The accompanying code in this article uses Azure Powershell and Azure CLI to implement the automation of the infrastructure. 


---

# Why did I write this article?

I wanted to demonstrate that if we stay committed to the fundamental tools and products like **Azure CLI**, **Powershell**, **Github** and **Azure Devops** - then a fairly complex process like CI/CD can be solved with relative ease. I often hear developers stressing how important it is to use a 3rd party Cloud management product like Terraform or Pulumi. If you are on **Azure** and have no intention for a multi-cloud solution, then CLI and PowerShell coupled with an automation agent like **Azure Devops**/**Jenkins** are all you need for your "Infrastructure as code" solution. 

---

# What sort of application are we automating in this article?

For the purpose of demonstrating a working CI/CD solution, I have developed a very simple application which comprises of the following:
- One REST end point implemented in a .NET Core Web API project. This end point generates dummy weather forecast records
- Static HTML served out of a storage account (with static web site option enabled). This invokes the REST end point and renders the forecast on the web browser

In a more practical scenario you would have other assets like a central database, Redis cache, message bus and most probably multiple micrososervices hosted in their respective web app.

#### Block diagram
![architecture](docs/images/application-block-diagram.png "app architecture")

#### Browser rendition
![architecture](docs/images/weather-forecast-browser.png "app architecture")

---

# What are the key technologies involved?

- **Github** - Source code repository
- **Azure Devops** - Build (CI) and release(CD) pipelines
- **Azure** - The cloud platform where the infrastructure is deployed
- **Azure app service** -  Azure serverless compute service for hosting a REST end point implemented on .NET Core Web API
- **Azure storage account** - Azure storage account configured for serving static HTML content over HTTP

---

# What is the desired development workflow?
A classic CI/CD workflow with deployments to **dev-uat-prod** stages would be as follows. The workflow could be even simpler or more complex. Example of complex CI/CD scenarios:
- Carry out integration test after deployment to Dev where the REST end points are tested
- Deploy to a QA environment for carrying out load tests
- Carry out UI automated UI tests immediately after deploying to Dev

![Dev workflow](docs/images/ci-cd-development-workflow.png "dev workflow")

---

# How do we structure the Github repo?
I have followed a [Monorepo](https://en.wikipedia.org/wiki/Monorepo) approach for the toy application. This was mainly for simplicity. If you are following a Monorepo then there are certain guidelines to be followed in the YAML file, so that the CI/CD automation environment (Azure Devops) can respond correctly to code commits in the source repository (Github). This is discussed further down.

```
|                       
+---BackEnd
|   +---build
|   |       build.yml
|   |       
|   +---infrastructure
|   |       common.ps1
|   |       createwebapp.ps1
|   |       deploy.ps1
|   |       
|   \---src
|                       
|           
+---FrontEnd
|   +---build
|   |       build.yml
|   |       
|   +---infrastructure
|   |       deploy.ps1
|   |       
|   \---src
|                   
```
You will notice that the repo is structured into 2 top level folders **BackEnd** and **FrontEnd** which contain a .NET Core WebApp and static HTML assets respectively.

Both the folders have their respective **build.yml** and **IAC** PowerShell scripts.

---

# How do we structure our assets on the Azure cloud?

I have separated the deployment into 6 resource groups. 3 resource groups for BackEnd and 3 resource groups for FrontEnd. Such a structure simplifies management and cost monitoring.

#### Breakdown of resource groups
![list of resource groups](docs/images/azure-resource-groups-list.png "list of resource groups")

#### BackEnd resource groups
![backend resource group](docs/images/azure-resource-group-backend.png "backend resource group")

#### FrontEnd resource groups
![front end resource group](docs/images/azure-resource-group-frontend.png "front end resource group")


---

# What is the responsibility of the Continuous Integration(CI) pipeline?

#### Overview
The CI pipeline is defined by a set of ordered tasks written down in a YAML file. The most common tasks performed by CI stage are:
1. code checkout
1. code compilation
1. unit tests
1. code coverage analysis
1. quality gates assessment (e.g. Sonar Cloud integration)

Example of a YAML snippet which executes the .NET unit tests in the **BackEnd** project

```
- task: DotNetCoreCLI@2
  displayName: Test
  inputs:
    command: test
    projects: '**/*[Tt]est*/*.csproj'
    arguments: '--configuration $(BuildConfiguration)'

```
#### What is not the responsibility of the CI pipeline?
I would refrain from deploying assets to the Cloud from the CI pipeline. Even though YAML allows you to specify Azure CLI/Powershell Tasks. This should be the responsibility of the CD pipeline

---

# What is the responsibility of the Continuous Integration(CD) pipeline?
#### Overview
The CD stage is responsible for collecting the build output from CI stage and then executing the IAC scripts. The CD is futher split into environment specific stages. It could get more complex. E.g. Kick of a load test after deployment to Dev environment is complete.

#### 3 stages of infrastructure deployment (dev/uat/prod)
![3 stages view](docs/images/cd-3-stages-view.png "3 stages view")


#### What is the difference between dev,uat and prod stages?

The steps are nearly identical. They all point to the same PowerShell scripts. So what is the difference? 
 -The variables differ. E.g. the variable *environment*
 -The service connection used by Devops to connect to the Azure cloud may differ. E.g. An UAT subscription for UAT environment and PROD subscription for production environment

The following snippet from *BackEnd\infrastructure\common.ps1 demonstrates* how we can use the environment to create our assets
```
$environment=$env:ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($environment)){
    Write-Error -Message "The variable 'environment' was empty"
}

$ResourceGroup="rg-$environment-demo-webapp-with-cicd"

$Location="uksouth"
$PlanName="WebAppPlanName"
$WebAppName="MyDemoWebApi123-$environment"

$StaticSiteStorageAccount="saustorageaccount001$environment"
$StaticSiteResourceGroup="rg-demo-staticwebsite-with-cicd"
```

#### What is not the responsibility of the CD pipeline?

Do not compile code here. Do not run unit tests. Let the CI pipeline handle this and be responsible for producing the drops

---

# How to create a CI pipeline in Azure Devops and link with code in Github ? 

![new ci pipeline](docs/images/ci-new-pipeline-from-github.png "new ci pipeline")

#### Essential points to remember
- Azure Devops will expect a YML file in your repo
- This file must be in the master branch if you want to test out the entire CI/CD flow
- Therefore you must get a working YML comitted into master before you can finish the rest of the CI/CD pipeline
- Remember to have `tigger` and `pr` elements if you are following a mono-repo approach

# How does Azure Devops automatically determine which CI pipeline to run ?
If you are following a **Monorepo** approach, then **Azure Devops** faces a challenge. Whenever a file is comitted to the Github repo, how would Devops know which CI pipeline to execute?

The YML specification addresses this problem by using the elements `trigger` and `pr`

#### Example snippet
Every YML should have something similar at the very beginning of the file. The `trigger` and `pr` settings work in tandem to guarantee execution of the correct YML file.

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
#### Why do we need a trigger element?
The `trigger` element places a path filter on every commit to the **master** branch. If any of the committed files match the specified pattern and the branch is master then the CI is kicked off.

#### Why do we need a pr element?
The `pr` element places a path filter on every commit to the **feature** branch which is under an active pull request. If any of the committed files in the feature branch match the specified pattern and the branch is under an active PR, then the CI is kicked off.

Every successful Pull Request ends with a merge of the **feature** branch into the **master**. The merge operation kicks of the CI once more. But this time it is on the **master** branch.


---

# How to configure the CD pipeline?


#### Overview
The CD stage requires configuration at several places
![cd what each block does](docs/images/cd-high-level-arrows.png "cd arrows")

#### Variable initialization
The variable **environment** (could be any name) influences the PowerShell script. The PowerShell script will use this environment variable to control the names of the assets in the cloud.

**Example**: In the following Powershell snippet, the Azure resource group containing our Azure Web app is named as per the environment. Notice that the naming of the environment is also led by the environment variable.
```
$environment=$env:ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($environment)){
    Write-Error -Message "The variable 'environment' was empty"
}

$ResourceGroup="rg-$environment-demo-webapp-with-cicd"
$Location="uksouth"
$PlanName="WebAppPlanName"
$WebAppName="MyDemoWebApi123-$environment"


```

![variables view](docs/images/cd-variables-view.png "variables view")

#### How does Azure Devops establish a connection with Azure cloud?

A one-time connection trust between Azure Devops and Azure cloud needs to be established. This is done via the **Service connections** panel of **Azure Devops**.

![new service connection](docs/images/cd-new-service-connection.png "new service connection")

You will also need to register a new **Application** in Active Directory


![app registration](docs/images/cd-application-registration.png "app registration")



#### How to specify the connection to Azure cloud?
The service connection created in the previous step is now available for use in any of the stages. 
**Example**: In the case of a **Azure CLI** task or a **Azure PowerShell** task, this can be specified in the drop down labelled *Azure Resource Manager Connection*. 
Pay attention to the check boxes. This is neccessary if we want to run Powershell scripts with **Azure CLI** and **Azure Powershell** commands.
![specify service connection](docs/images/cd-specify-service-connection.png "specify service connection")


#### What are the tasks in each of the stages?

![tasks in each stage](docs/images/cd-tasks-list-with-explanation.png "tasks in each stage")


#### How to trigger the CD?
For full automation, the CD should be configured to "listen" to drops from the CI pipeline.
![cd trigger](docs/images/cd-specify-triggers.png "cd trigger")

#### How to respond to CI builds from  master branch and Pull Request branch?
![cd trigger definition](docs/images/cd-define-trigger.png "cd trigger definition")


#### How to ensure PROD follows UAT and UAT follows DEV?
![cd trigger definition](docs/images/cd-uat-after-dev-prod-after-uat.png "cd trigger definition")


#### How to create an Approval for the Prod stage?

![cd prod approval](docs/images/cd-define-prod-approval.png "cd prod approval")

---

# What happens when a Pull Request is raised?
In this section we will minutely examine the Pull Request workflow. To drive this example, we will make a code commit to our toy BackEnd application.

#### Create a feature branch

```
git branch feature/do-logging
git checkout feature/do-logging
```

#### Write the code
For this excercise, I have simply added 1 line of logging to the API end point.
![vs diff](docs/images/pr-demo-code-change-diff.png "vs diff")

#### Push the changes
![vs push the changes](docs/images/pr-demo-git-push.png "vs push the changes")

#### How does Github react after pushing the changes?

![github view new branch](docs/images/pr-demo-github-newbranch-created-pr-button.png "github view new branch")

#### Create a pull request on the feature branch

![create pull request](docs/images/pr-demo-github-create-pull-request.png "create pull request")

#### How does Azure Devops react when a new Pull Request is created on Github?
Notice that the BackEnd CI has kicked off automatically. The CI is running on the feature branch.
![ci running](docs/images/pr-demo-devops-backend-ci-running.png "ci running")

When the CI completes, Azure Devops kicks off the CD stage.
![cd running](docs/images/pr-demo-devops-backend-cd-running-pr-branch.png "cd running")

#### How does Github react when Azure Devops is executing the CI and CD?

Notice that Github is reporting that **BackEnd CI** and **BackEnd CD** are mandatory checks.
![pr checks required](docs/images/pr-demo-github-pull-request-checks-required.png "pr checks required")

#### How does Github react when CI and CD has completed on Azure Devops?

![pr checks complete](docs/images/pr-demo-github-pull-request-checks-completed.png "pr checks complete")

Note that Azure Devops has completed the CD stage on the feature branch.
![to be done](docs/images/pr-demo-devops-backend-cd-complete-pr-branch.png "to be done")

#### Complete the Pull Request


![complete the pr](docs/images/pr-demo-github-merge-pull-request-admin-privileges.png "complete the pr")

#### Wait for CI on master branch
Github has merged the Pull Request. Azure Devops will kick off the CI once more. This time on the **master** branch
![ci on master branch](docs/images/pr-demo-devops-backend-ci-running-master-branch.png "ci on master branch")

#### Wait for CD on master

DEV stage of CD running
![cd on master dev](docs/images/pr-demo-devops-backend-cd-running-master-branch-dev-stage.png "cd on master dev")

DEV stage is complete.UAT stage of CD is now running
![cd on master uat](docs/images/pr-demo-devops-backend-cd-running-master-branch-uat-stage.png "cd on master uat")

UAT stage is complete. PROD stage waiting for approval
![cd on prod waiting for approval](docs/images/pr-demo-devops-backend-cd-running-master-branch-prod-approval-pending-stage.png "cd on prod waiting for approval")

Click on the Approve button

![prod click on approve](docs/images/pr-demo-devops-backend-cd-running-master-branch-prod-approve-click.png "prod click on approve")

PROD stage of CD is now complete
![cd on prod complete](docs/images/pr-demo-devops-backend-cd-running-master-branch-prod-complete.png "cd on prod complete")


#### Evidence of deployment on Azure
How do we know that our BackEnd assets have actually been updated on Azure cloud? The Activity Log of the **App Service** displays the following:
![azure activity](docs/images/pr-demo-azure-activity.png "azure activity")



---


# How to test the infrastructure code by running it on your workstation?
This is absolutely important. You want to run all the infrastructure scripts from your local workstation before you can expect CI/CD to work smoothly.
You want a system where there is quick feedback to the developer. CI/CD is an automation engine and should not be used as an debugging environment.
#### Getting Azure CLI to work?
You will need to do a `az login`. This will do a one time interactive login into Azure. 
All subsequent calls to `az` CLI will use saved credentials to interact with Azure.
https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli#sign-in-interactively

You can verify by opening another Powershell console and typing `az account show`. 

#### Getting Azure Powershell to work?
You will need to do a `Set-AzContext` from a Powershell Core shell. This will do a one time interactive login into Azure.
All subsequent Azure powershell cmdlet invocations will use saved credentials to interact with Azure.
https://docs.microsoft.com/en-us/powershell/module/az.accounts/set-azcontext?view=azps-7.2.0

You can verify by opening another Powershell Core shell and run `Get-AzContext`

**Attention!** Azure CLI and Azure Powershell require their own authentication and hence you need to do both of the above.

# Getting started with the accompanying code

#### Link to accompanying source code
https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd

#### Building and deploying the BackEnd from your local workstation
The back end code is managed by the single .SLN file
https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/BackEnd/src/Demo.sln

![cd prod approval](docs/images/code-solution-explorer.png "cd prod approval")

#### How to create the Azure assets for the back end?
1. Execute the following PS script from a Powershell Core shell.
1. The script will create the resource group, app service plan and app service.
https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/BackEnd/infrastructure/createwebapp.ps1

#### How to deploy the binaries of the BackEnd to Azure?
1. Build the solution
1. Zip the binaries
1. **Attention!** The ZIP file should contain the assemblies at the very top level
1. Use the script https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/BackEnd/infrastructure/deploy.ps1


#### Building and deploying the FrontEnd on your local workstation
The FrontEnd comprises of a simple HTML and JS files. Hence there is no build required - unlike a React or Vue application.
1. Execute the following PS script from a Powershell Core shell.
1. The script will upload the static content to the Azure blob container `$web`
https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/FrontEnd/infrastructure/deploy.ps1


#### Creating a CI pipeline
You will need to create two CI pipelines in your Azure Devops account. These should use the following YML files
1. https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/FrontEnd/build/build.yml
1. https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd/BackEnd/build/build.yml

#### Creating a CD pipeline
You will need to create 2 CD releases. One wired up with the BackEnd CI and another wired up with the FrontEnd CI.

#### Setting the Github branch policy
To see a full Pull Request workflow in action, protection must be applied to the **branch** branch.

![cd prod approval](docs/images/github-master-branch-protection.png "cd prod approval")




# Important snippets of PowerShell and CLI for creating assets on the Azure Cloud?
Refer the PowerShell scripts inside the folders **\BackEnd\Infrastructure**  and **\FrontEnd\infrastructure** for the complete Powershell script. 

#### Creating a resource group
```
New-AzResourceGroup -Name $ResourceGroup  -Location $Location -Force
```

#### Creating a App Service Plan
```
function CreatePlan(){
    Write-Host "Creating plan $PlanName"
    az appservice plan create --name $PlanName --resource-group $ResourceGroup --sku $PlanSKu --number-of-workers $NumOfWorkers --subscription $ctx.Subscription.Id
}

```

#### Creating an Web App Service
```
function CreateWebApp(){
    Write-Host "Creating Web App $WebAppName"
    az webapp create --name $WebAppName --plan $PlanName --resource-group $ResourceGroup --subscription $ctx.Subscription.Id    
}
```

#### Creating a Storage Account
```
Write-Host "Creating storage account $StaticSiteStorageAccount"
az storage account create --name $StaticSiteStorageAccount --resource-group $ResourceGroup --location $Location --sku Standard_LRS  --subscription $ctx.Subscription.Id
```

### Uploading static HTML files to the storage account
```
Write-Host "Uploading files from $Sourcefolder"
az storage blob upload-batch --account-name $StaticSiteStorageAccount --source $Sourcefolder -d '$web'
```


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


#### How to build and publish from command line?

```
dotnet publish --configuration Release --output c:\truetemp\someoutput\ demo.sln
```

Specifying the file version helps immensely in post deployment application support. 
```
dotnet publish --output c:\truetemp\someoutput\ /p:FileVersion=1.2.3.4 demo.sln
```


#### What is a Monorepo?
https://en.wikipedia.org/wiki/Monorepo



# Further thoughts - extending the toy application (to be done)

#### How would the scripts change if we wanted higher scalability in PROD environment?
to be done

#### How would the design change if we were to add more micro services?
to be done

#### How would the repository design change if there were central resources like MSSQL, Redis and KeyVault in the mix?
to be done




Talk about 
- the source code structure if there were to be a database
- How would we generate settings
- Emphasize on key vault 
