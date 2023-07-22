[[_TOC_]]

# Overview
We want to demonstrate how to create a CI CD pipeline using YAML which has a BUILD, DEV-DELOY and PROD-DEPLOY stage

[[Show a mermaid diagram ]]


---


# Overview
My experiments with producing a Devop variables and expressions and producing a semantic release number

# Objective
Can we stick to out of box and not use a 3 rd party solution like Git version


# Things to do
1. Change from `blah`
1. TEMPLATE FOR BUILD
1. TEPLATE FOR DEPLOY
1. POWERSHELL TO DSISPLAY SECRET

# Some more things to do
1. C# project
1. Dotnet build, with build number  
1. archive and publish
1. Python project and 


# Notes

## Build
```
dotnet build MyDemoCSharp101.sln /p:version=1.2.3-betaaaa.4
```

## Publish
```dotnetcli
dotnet publish src\MyDemoCSharp101\MyDemoCSharp101.sln /p:version=1.2.3.4 --configuration Release
``````

# References

## Good article on Template Parameters
https://damienaicheh.github.io/azure/devops/2021/02/10/variable-templates-azure-devops-en.html


----

# Restructuring plan

## Step 1-Simple skeletal CI/CD YAML with 1 BUILD and 1 DEV_DEPLOY stages
to be done
What are we showing here ?
- 1 master YAML
- Split into stages (BUILD and DEV_DEPLOY)
- Using Build and Deploy templates
- Specify 

## Step 1.3-Condition on PROD_DEPLOY stage
What are we showing here?
- Add a PROD_DEPLOY stage
- Add a condition for branch==main

## Step 1.5-Semantic versioning
What are we showing here?
- Simple `if` condition to set the Build.Buildnumber based on branch

## Step 2-Pass parameters to the Deploy stage
- 1 parameter in the Release template (name=environment)
- DEV stage
- PROD stage

## Step 3-Pass variables from variable group
What are we showing here ?
- We are passing another variable besides the 'environment'
- 1 Devops variable group
- 2 variables (dev_cnstring, prod_cnstring)
- Pass the dev_cnstring as a a
- dev cn string
- prod cn string
